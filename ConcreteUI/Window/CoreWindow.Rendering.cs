using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Hosting;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Graphics.Native.DXGI;
using ConcreteUI.Internals;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using Microsoft.Win32;

using WitherTorch.Common;
using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;
using WitherTorch.Common.Windows.Structures;

using ContextMenu = ConcreteUI.Controls.ContextMenu;
using ToolTip = ConcreteUI.Controls.ToolTip;

namespace ConcreteUI.Window
{
    public abstract partial class CoreWindow : IRenderer
    {
        #region Enums
        [Flags]
        private enum UpdateFlags : long
        {
            None = 0,
            ChangeTitle = 0b1,
        }

        protected enum Brush : int
        {
            TitleBackBrush,
            TitleForeBrush,
            TitleForeDeactiveBrush,
            TitleCloseButtonActiveBrush,
            _Last,
        }
        #endregion

        #region Static Fields
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "app.title.back",
            "app.title.fore.active",
            "app.title.fore.deactive",
            "app.title.closeButton.active",
        };
        private static readonly Pool<LayoutEngine> _layoutEnginePool = new Pool<LayoutEngine>(1);
        private static readonly LazyTiny<GraphicsDeviceProvider> graphicsDeviceProviderLazy
            = new LazyTiny<GraphicsDeviceProvider>(CreateGraphicsDeviceProvider, LazyThreadSafetyMode.ExecutionAndPublication);
        #endregion

        #region Fields
        private readonly ConcurrentDictionary<Type, UIElement> _overlayElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly ConcurrentDictionary<Type, UIElement> _backgroundElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly UnwrappableList<UIElement> _overlayElementList = new UnwrappableList<UIElement>();
        private readonly UnwrappableList<UIElement> _backgroundElementList = new UnwrappableList<UIElement>();
        private readonly WindowMaterial _windowMaterial;
        private SwapChainGraphicsHost _host;
        private DirtyAreaCollector _collector;
        private RenderingController _controller;
        private UIElement _focusElement;
        private ThemeResourceProvider _resourceProvider;
        private float baseLineWidth = 1.0f;
        private bool isShown, isInitializingElements;
        private long _updateFlags = Booleans.TrueLong;
        #endregion

        #region Rendering Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private D2D1DeviceContext _deviceContext;
        private DWriteTextLayout _titleLayout;
        protected D2D1ColorF _clearDCColor, _windowBaseColor;
        protected RectF _minRect, _maxRect, _closeRect, _pageRect, _titleBarRect;
        protected BitVector64 _titleBarButtonStatus, _titleBarButtonChangedStatus;
        protected float _drawingOffsetX, _drawingOffsetY, _drawingBorderWidth;
        #endregion

        #region Properties
        public new ContextMenu ContextMenu => GetOverlayElement<ContextMenu>();
        public ToolTip ToolTip => GetBackgroundElement<ToolTip>();
        public UIElement FocusedElement => _focusElement;
        public WindowMaterial WindowMaterial => _windowMaterial;
        #endregion

        #region Events
        public event EventHandler<ContextMenu> ContextMenuChanging;

        public event EventHandler<UIElement> FocusElementChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnContextMenuChanging(ContextMenu newContextMenu) => ContextMenuChanging?.Invoke(this, newContextMenu);
        #endregion

        #region Init
        private static GraphicsDeviceProvider CreateGraphicsDeviceProvider()
        {
            string targetGpuName = ConcreteSettings.TargetGpuName;
            if (string.IsNullOrEmpty(targetGpuName))
                return new GraphicsDeviceProvider(DXGIGpuPreference.Invalid);
            if (targetGpuName.StartsWith('#'))
            {
                DXGIGpuPreference preference = targetGpuName switch
                {
                    ConcreteSettings.ReservedGpuName_Default => DXGIGpuPreference.Unspecified,
                    ConcreteSettings.ReservedGpuName_MinimumPower => DXGIGpuPreference.MinimumPower,
                    ConcreteSettings.ReservedGpuName_HighPerformance => DXGIGpuPreference.HighPerformance,
                    _ => DXGIGpuPreference.Invalid,
                };
                return new GraphicsDeviceProvider(preference);
            }
            return new GraphicsDeviceProvider(targetGpuName);
        }

        [Inline(InlineBehavior.Remove)]
        private void InitRenderObjects()
        {
            WindowMaterial material = _windowMaterial;
            CoreWindow parent = _parent;
            SwapChainGraphicsHost host;
            if (parent is null)
                host = GraphicsHostHelper.CreateSwapChainGraphicsHost(Handle, graphicsDeviceProviderLazy.Value,
                    useFlipModel: material == WindowMaterial.None && SystemConstants.VersionLevel >= SystemVersionLevel.Windows_8);
            else
                host = GraphicsHostHelper.FromAnotherSwapChainGraphicsHost(parent._host, Handle);
            _host = host;
            _collector = DirtyAreaCollector.TryCreate(host as SwapChainGraphicsHost1);
            D2D1DeviceContext deviceContext = host.BeginDraw();
            int dpi = Dpi;
            if (dpi != 96)
                deviceContext.Dpi = new PointF(dpi, dpi);
            _deviceContext = deviceContext;
            ChangeBackgroundElement(new ToolTip(this, element => GetRenderingElements().Contains(element)));
            isInitializingElements = true;
            InitializeElements();
            isInitializingElements = false;
            ApplyTheme(parent is null ? new ThemeResourceProvider(deviceContext, ThemeManager.CurrentTheme, _windowMaterial) : parent._resourceProvider);
            SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }
        #endregion

        #region Override Methods
        protected override void OnShown(EventArgs e)
        {
            ConcreteUtils.ApplyWindowStyle(this, ref _fixLagObject);
            UpdateFirstTime();
            base.OnShown(e);
            BeginInvoke(OnShown2);
        }

        private void OnShown2()
        {
            Point point = PointToClientBase(MousePosition);
            OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.Cancel)
                return;

            SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

            RenderingController controller = _controller;
            if (controller is not null)
            {
                controller.Stop();
                controller.WaitForExit(500);
            }

            SwapChainGraphicsHost host = _host;
            if (host is not null && !host.IsDisposed)
                host.EndDraw();
        }
        #endregion

        #region Implements Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GraphicsDeviceProvider GetGraphicsDeviceProvider() => _host.GetDeviceProvider();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1DeviceContext GetDeviceContext() => _deviceContext;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderingController GetRenderingController() => _controller;

        public virtual void RenderElementBackground(UIElement element, D2D1DeviceContext context)
            => context.Clear(_clearDCColor);

        void IRenderingControl.Render(RenderingFlags flags)
        {
            bool resized = (flags & RenderingFlags.Resize) == RenderingFlags.Resize,
                redrawAll = (flags & RenderingFlags.RedrawAll) == RenderingFlags.RedrawAll;
            redrawAll = !RenderCore(redrawAll, resized);
            if (redrawAll)
                _controller.RequestUpdate(true);
        }

        ToolTip IRenderer.GetToolTip() => GetBackgroundElement<ToolTip>();

        bool IRenderer.IsInitializingElements() => isInitializingElements;

        public float GetBaseLineWidth() => baseLineWidth;
        #endregion

        #region Abstract Methods
        protected abstract void InitializeElements();
        protected abstract IEnumerable<UIElement> GetRenderingElements();
        #endregion

        #region Virtual Methods
        protected virtual void ApplyThemeCore(ThemeResourceProvider provider)
        {
            _clearDCColor = provider.TryGetColor(ThemeConstants.ClearDCColorNode, out D2D1ColorF color) ? color : default;
            _windowBaseColor = provider.TryGetColor(ThemeConstants.WindowBaseColorNode, out color) ? color : default;
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            UIElementHelper.ApplyTheme(provider, _overlayElementList);
            UIElementHelper.ApplyTheme(provider, _backgroundElementList);
            if (_brushes[(int)Brush.TitleBackBrush] is D2D1SolidColorBrush backBrush &&
                _windowMaterial == WindowMaterial.Integrated && SystemConstants.VersionLevel >= SystemVersionLevel.Windows_11_21H2)
                FluentHandler.SetTitleBarColor(Handle, (Color)backBrush.Color);
        }

        protected virtual void OnMouseDownForElements(in MouseInteractEventArgs args)
        {
            bool allowRegionalMouseEvent = true;
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseDownForElements(elements, args, ref allowRegionalMouseEvent);
            UIElementHelper.OnMouseDownForElements(GetBackgroundElements(), args, ref allowRegionalMouseEvent);
        }

        protected virtual void OnMouseMoveForElements(in MouseInteractEventArgs args)
        {
            Cursor predicatedCursor = null;
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseMoveForElements(elements, args, ref predicatedCursor);
            UIElementHelper.OnMouseMoveForElements(GetBackgroundElements(), args, ref predicatedCursor);
            if (predicatedCursor is null)
                Cursor = DefaultCursor;
            else
                Cursor = predicatedCursor;
        }

        protected virtual void OnMouseUpForElements(in MouseInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseUpForElements(elements, args);
            UIElementHelper.OnMouseUpForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnMouseScrollForElements(in MouseInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseScrollForElements(elements, args);
            UIElementHelper.OnMouseScrollForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnKeyDownForElements(KeyEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnKeyDownForElements(elements, args);
            UIElementHelper.OnKeyDownForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnKeyUpForElements(KeyEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnKeyUpForElements(elements, args);
            UIElementHelper.OnKeyUpForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnCharacterInputForElements(char character)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnCharacterInputForElements(elements, character);
            UIElementHelper.OnCharacterInputForElements(GetBackgroundElements(), character);
        }

        protected virtual void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
            {
                SizeF size = ClientSize;
                _pageRect = GraphicsUtils.AdjustRectangleF(RectF.FromXYWH(0, 0, size.Width, size.Height));
            }
            else
            {
                float windowScaleFactor = this.windowScaleFactor;
                float drawingBorderWidth;
                float drawingOffsetX, drawingOffsetY;
                if (WindowState == FormWindowState.Maximized)
                {
                    drawingBorderWidth = _drawingBorderWidth = 0;
                    if (windowScaleFactor != 1.0f)
                    {
                        drawingOffsetX = -DesktopLocation.X * windowScaleFactor;
                        drawingOffsetY = -DesktopLocation.Y * windowScaleFactor;
                    }
                    else
                    {
                        drawingOffsetX = -DesktopLocation.X;
                        drawingOffsetY = -DesktopLocation.Y;
                    }
                }
                else
                {
                    drawingBorderWidth = _borderWidth * ((IRenderer)this).GetBaseLineWidth();
                    _drawingBorderWidth = drawingBorderWidth;
                    drawingOffsetX = 0;
                    drawingOffsetY = 0;
                }
                _drawingOffsetX = drawingOffsetX;
                _drawingOffsetY = drawingOffsetY;
                float x = windowSize.Width - 1 - drawingOffsetX, y = drawingOffsetY;
                _closeRect = RectF.FromXYWH(x -= UIConstants.TitleBarButtonSizeWidth, y, UIConstants.TitleBarButtonSizeWidth, UIConstants.TitleBarButtonSizeHeight);
                _maxRect = RectF.FromXYWH(x -= UIConstants.TitleBarButtonSizeWidth, y, UIConstants.TitleBarButtonSizeWidth, UIConstants.TitleBarButtonSizeHeight);
                _minRect = RectF.FromXYWH(x - UIConstants.TitleBarButtonSizeWidth, y, UIConstants.TitleBarButtonSizeWidth, UIConstants.TitleBarButtonSizeHeight);
                RectF titleBarRect = _titleBarRect = RectF.FromXYWH(drawingOffsetX + 1, drawingOffsetY + 1, Width - 2, 26);
                _pageRect = GraphicsUtils.AdjustRectangleF(new RectF(drawingOffsetX + drawingBorderWidth, titleBarRect.Bottom + 1,
                    windowSize.Width - drawingOffsetX - drawingBorderWidth, windowSize.Height - drawingBorderWidth));
            }
            if (callRecalculatePageLayout && _pageRect.IsValid)
                RecalculatePageLayout((Rect)_pageRect);
        }

        protected virtual void RecalculatePageLayout(in Rect pageRect)
        {
            LayoutEngine layoutEngine = RentLayoutEngine();
            layoutEngine.RecalculateLayout(pageRect, GetRenderingElements());
            layoutEngine.RecalculateLayout(pageRect, GetOverlayElements());
            ReturnLayoutEngine(layoutEngine);
        }
        #endregion

        #region Rendering
        protected IEnumerable<UIElement> GetOverlayElements() => _overlayElementList;

        protected IEnumerable<UIElement> GetBackgroundElements() => _backgroundElementList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected D2D1Brush GetBrush(Brush brush) => _brushes[(int)brush];

        [Inline]
        private bool RenderCore(bool force, bool resized)
        {
            bool isSizeChanged = false;
            SwapChainGraphicsHost host = _host;
            if (resized)
            {
                if (host is null || host.IsDisposed)
                    return false;
                isSizeChanged = force = true;
                Size size = base.ClientSize;
                host.Resize(size);
                RecalculateLayout(ScalingSizeF(size, windowScaleFactor), true);
            }
            D2D1DeviceContext deviceContext = host.GetDeviceContext();
            if (deviceContext is null || deviceContext.IsDisposed)
                return true;
            DirtyAreaCollector rawCollector = force ? null : _collector;
            DirtyAreaCollector collector = rawCollector ?? DirtyAreaCollector.Empty;
            RenderTitle(deviceContext, collector, force);
            RectF pageRect = _pageRect;
            if (!pageRect.IsValid)
                return true;
            RenderPage(deviceContext, collector, pageRect, force);
            host.Flush();
            if (rawCollector is null)
            {
                if (ConcreteSettings.UseDebugMode)
                {
                    host.Present();
                    return true;
                }
                return host.TryPresent();
            }
            if (ConcreteSettings.UseDebugMode)
            {
                rawCollector.Present(dpiScaleFactor);
                return true;
            }
            return rawCollector.TryPresent(dpiScaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect)
            => deviceContext.Clear(_windowBaseColor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderOnceContent(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPage(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect, bool force)
        {
            deviceContext.PushAxisAlignedClip(pageRect, D2D1AntialiasMode.Aliased);
            if (force)
            {
                collector.UsePresentAllModeOnce();
                RenderPageBackground(deviceContext, collector, pageRect);
                RenderOnceContent(deviceContext, collector, pageRect);
            }
            UIElementHelper.RenderElements(collector, GetRenderingElements(), ignoreNeedRefresh: force);
            UIElementHelper.RenderElements(collector, GetOverlayElements(), ignoreNeedRefresh: force || collector.HasAnyDirtyArea());
            deviceContext.PopAxisAlignedClip();
        }

        protected virtual void ClearDCForTitle(D2D1DeviceContext deviceContext)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
            {
                deviceContext.Clear();
                return;
            }
            GraphicsUtils.ClearAndFill(deviceContext, _brushes[(int)Brush.TitleBackBrush], _clearDCColor);
        }

        protected virtual void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
                return;
            D2D1Brush[] brushes = _brushes;

            BitVector64 TitleBarButtonChangedStatus = _titleBarButtonChangedStatus;
            BitVector64 titleBarStates = this.titleBarStates;
            _titleBarButtonChangedStatus.Reset();
            #region 繪製標題
            if (force)
            {
                DWriteTextLayout titleLayout;
                if ((Interlocked.Exchange(ref _updateFlags, Booleans.FalseLong) & (long)UpdateFlags.ChangeTitle) == (long)UpdateFlags.ChangeTitle)
                {
                    DWriteFactory factory = SharedResources.DWriteFactory;
                    DWriteTextFormat titleFormat = Interlocked.Exchange(ref _titleLayout, null);
                    if (titleFormat is null || titleFormat.IsDisposed)
                    {
                        titleFormat = factory.CreateTextFormat(_resourceProvider.FontName, UIConstants.TitleFontSize);
                        titleFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
                    }
                    titleLayout = GraphicsUtils.CreateCustomTextLayout(_text, titleFormat, 26);
                    titleFormat.Dispose();
                    DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
                }
                else
                {
                    titleLayout = _titleLayout;
                }
                ClearDCForTitle(deviceContext);
                if (titleBarStates[0])
                {
                    deviceContext.PushAxisAlignedClip(_titleBarRect, D2D1AntialiasMode.Aliased);
                    deviceContext.DrawTextLayout(new PointF(_drawingOffsetX + 7.5f, _drawingOffsetY + 1.5f), titleLayout, brushes[(int)Brush.TitleForeBrush]);
                    deviceContext.PopAxisAlignedClip();
                }
            }
            BitVector64 TitleBarButtonStatus = _titleBarButtonStatus;
            FontIconResources iconStorer = FontIconResources.Instance;
            if (FormBorderStyle == FormBorderStyle.Sizable)
            {
                if (titleBarStates[1] && (TitleBarButtonChangedStatus[0] || force))
                {
                    RectF minRect = _minRect;
                    deviceContext.PushAxisAlignedClip(minRect, D2D1AntialiasMode.Aliased);
                    if (!force)
                        ClearDCForTitle(deviceContext);
                    iconStorer.RenderMinimizeButton(deviceContext, minRect.Location,
                        TitleBarButtonStatus[0] ? brushes[(int)Brush.TitleForeBrush] : brushes[(int)Brush.TitleForeDeactiveBrush]);
                    deviceContext.PopAxisAlignedClip();
                    collector.MarkAsDirty(minRect);
                }
                if (titleBarStates[2] && (TitleBarButtonChangedStatus[1] || force))
                {
                    RectF maxRect = _maxRect;
                    deviceContext.PushAxisAlignedClip(maxRect, D2D1AntialiasMode.Aliased);
                    if (!force)
                    {
                        ClearDCForTitle(deviceContext);
                    }
                    D2D1Brush foreBrush = TitleBarButtonStatus[1] ? brushes[(int)Brush.TitleForeBrush] : brushes[(int)Brush.TitleForeDeactiveBrush];
                    if (_isMaximized)
                        iconStorer.RenderRestoreButton(deviceContext, maxRect.Location, foreBrush);
                    else
                        iconStorer.RenderMaximizeButton(deviceContext, maxRect.Location, foreBrush);
                    collector.MarkAsDirty(maxRect);
                    deviceContext.PopAxisAlignedClip();
                }
            }
            if (TitleBarButtonChangedStatus[2] || force)
            {
                RectF closeRect = _closeRect;
                deviceContext.PushAxisAlignedClip(closeRect, D2D1AntialiasMode.Aliased);
                if (!force)
                {
                    ClearDCForTitle(deviceContext);
                }
                iconStorer.RenderCloseButton(deviceContext, closeRect.Location,
                        TitleBarButtonStatus[2] ? brushes[(int)Brush.TitleForeBrush] : brushes[(int)Brush.TitleForeDeactiveBrush]);
                deviceContext.PopAxisAlignedClip();
                collector.MarkAsDirty(closeRect);
            }
            #endregion
        }
        #endregion

        #region Event Handlers
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) => _controller.Unlock();

        private void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e) => _controller.Lock();

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    _controller.Lock();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    _controller.Unlock();
                    break;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    _controller.Lock();
                    break;
                case PowerModes.Resume:
                    _controller.Unlock();
                    break;
            }
        }
        #endregion

        #region Public Methods

        private void ResetBlur()
        {
            if (InvokeRequired)
                Invoke(ResetBlur);
            else
                ConcreteUtils.ResetBlur(this);
        }

        private void UpdateFirstTime()
        {
            isShown = true;
            _controller = new RenderingController(this);
            UpdateInternal();
        }

        [Inline(InlineBehavior.Remove)]
        private void UpdateInternal() => _controller.RequestUpdate(true);

        [Inline(InlineBehavior.Remove)]
        private void OnDpiChangedRenderingPart()
        {
            SwapChainGraphicsHost host = _host;
            if (host is null || host.IsDisposed)
                return;
            D2D1DeviceContext context = host.GetDeviceContext();
            int dpi = Dpi;
            context.Dpi = new PointF(dpi, dpi);
            _controller.RequestUpdate(true);
        }

        [Inline(InlineBehavior.Remove)]
        private void OnWindowStateChangingRenderingPart(FormWindowState windowState)
        {
            RenderingController controller = _controller;
            if (controller is null)
                return;
            switch (windowState)
            {
                case FormWindowState.Maximized:
                    {
                        controller.RequestUpdate(true);
                        if (_windowState == FormWindowState.Minimized)
                            controller.Unlock();
                    }
                    break;
                case FormWindowState.Normal:
                    {
                        controller.RequestUpdate(true);
                        if (_windowState == FormWindowState.Minimized)
                            controller.Unlock();
                    }
                    break;
                case FormWindowState.Minimized:
                    {
                        controller.Lock();
                    }
                    break;
            }
        }

        private void CloseContextMenu(object sender, EventArgs e) => ChangeOverlayElement(typeof(ContextMenu), null);
        #endregion

        #region Normal Methods
        protected void TriggerResize() => _controller?.RequestResize();

        protected new void Update()
        {
            if (!isShown || _controller is null)
                return;
            UpdateInternal();
        }

        protected new void Refresh()
        {
            if (!isShown)
                return;
            _controller?.RequestUpdate(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static LayoutEngine RentLayoutEngine() => _layoutEnginePool.Rent();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void ReturnLayoutEngine(LayoutEngine engine) => _layoutEnginePool.Return(engine);

        public void ChangeFocusElement(UIElement element)
        {
            if (_focusElement == element)
                return;
            _focusElement = element;
            FocusElementChanged?.Invoke(this, element);
        }

        protected void ClearFocusElement()
        {
            _focusElement = null;
            FocusElementChanged?.Invoke(this, null);
        }

        public void ClearFocusElement(UIElement elementForValidation)
        {
            if (_focusElement != elementForValidation)
                return;
            ClearFocusElement();
        }

        protected T GetOverlayElement<T>() where T : UIElement => GetOverlayElement(typeof(T)) as T;

        protected UIElement GetOverlayElement(Type type) => _overlayElementDict.GetOrDefault(type, null);

        protected T ChangeOverlayElement<T>(T element, Predicate<T> predicate = null) where T : UIElement
        {
            Predicate<UIElement> translatedPredicate;
            if (predicate is null)
                translatedPredicate = null;
            else
                translatedPredicate = obj => obj is not T castedObj || predicate(castedObj);
            return ChangeOverlayElement(typeof(T), element, translatedPredicate) as T;
        }

        protected UIElement ChangeOverlayElement(Type type, UIElement element, Predicate<UIElement> predicate = null)
        {
            ConcurrentDictionary<Type, UIElement> overlayElementDict = _overlayElementDict;
            UnwrappableList<UIElement> overlayElementList = _overlayElementList;
            UIElement result;
            if (element is null)
            {
                if (overlayElementDict.TryRemove(type, out result))
                {
                    overlayElementList.Remove(result);
                    Update();
                }
                return result;
            }
            if (!overlayElementDict.TryGetValue(type, out result))
            {
                overlayElementDict.TryAdd(type, element);
                overlayElementList.Add(element);
                element.ApplyTheme(_resourceProvider);
                Update();
                return null;
            }
            if (result is null || predicate is null || predicate.Invoke(result))
            {
                int index = overlayElementList.IndexOf(result);
                overlayElementDict[type] = element;
                if (index > -1)
                {
                    overlayElementList[index] = element;
                }
                else
                {
                    overlayElementList.Add(element);
                }
                Update();
                return result;
            }
            return null;
        }

        protected T GetBackgroundElement<T>() where T : UIElement => GetBackgroundElement(typeof(T)) as T;

        protected UIElement GetBackgroundElement(Type type) => _backgroundElementDict.GetOrDefault(type, null);

        protected T ChangeBackgroundElement<T>(T element, Predicate<T> predicate = null) where T : UIElement
        {
            Predicate<UIElement> translatedPredicate;
            if (predicate is null)
                translatedPredicate = null;
            else
                translatedPredicate = obj => obj is not T castedObj || predicate(castedObj);
            return ChangeBackgroundElement(typeof(T), element, translatedPredicate) as T;
        }

        protected UIElement ChangeBackgroundElement(Type type, UIElement element, Predicate<UIElement> predicate = null)
        {
            ConcurrentDictionary<Type, UIElement> backgroundElementDict = _backgroundElementDict;
            UnwrappableList<UIElement> backgroundElementList = _backgroundElementList;
            UIElement result;
            if (element is null)
            {
                if (backgroundElementDict.TryRemove(type, out result))
                {
                    backgroundElementList.Remove(result);
                    Update();
                }
                return result;
            }
            if (!backgroundElementDict.TryGetValue(type, out result))
            {
                backgroundElementDict.TryAdd(type, element);
                backgroundElementList.Add(element);
                element.ApplyTheme(_resourceProvider);
                Update();
                return null;
            }
            if (result is null || predicate is null || predicate.Invoke(result))
            {
                int index = backgroundElementList.IndexOf(result);
                backgroundElementDict[type] = element;
                if (index > -1)
                    backgroundElementList[index] = element;
                else
                    backgroundElementList.Add(element);
                element.ApplyTheme(_resourceProvider);
                Update();
                return result;
            }
            return null;
        }

        public void OpenContextMenu(ContextMenu.ContextMenuItem[] items, Point location)
        {
            if (items.HasAnyItem())
            {
                ContextMenu contextMenu = new ContextMenu(this, items);
                contextMenu.ItemClicked += CloseContextMenu;
                contextMenu.Location = location;
                if (location.X + contextMenu.Width >= Width + _drawingOffsetX * 2)
                {
                    contextMenu.X = location.X - contextMenu.Width + 1;
                }
                if (location.Y + contextMenu.Height >= Height + _drawingOffsetY * 2)
                {
                    contextMenu.Y = location.Y - contextMenu.Height + 1;
                }
                ChangeOverlayElement(contextMenu)?.Dispose();
            }
        }

        [Inline(InlineBehavior.Remove)]
        public void CloseOverlayElement(UIElement elementForValidate)
            => CloseOverlayElement(elementForValidate.GetType(), elementForValidate);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseOverlayElement(Type elementType, UIElement elementForValidate) => (ChangeOverlayElement(elementType, null, _element => Equals(_element, elementForValidate)) as IDisposable)?.Dispose();

        protected void ApplyTheme(ThemeResourceProvider provider)
        {
            RenderingController controller = _controller;
            controller?.Lock();
            DisposeHelper.SwapDisposeInterlocked(ref _resourceProvider, provider);
            ApplyThemeCore(provider);
            WeakReference<CoreWindow>[] windowListSnapshot = GetWindowListSnapshot(_childrenReferenceList, out ArrayPool<WeakReference<CoreWindow>> pool, out int count);
            for (int i = 0; i < count; i++)
            {
                if (!windowListSnapshot[i].TryGetTarget(out CoreWindow window) || window is null)
                    continue;
                D2D1DeviceContext deviceContext = window._deviceContext;
                if (deviceContext is null)
                    continue;
                window.ApplyTheme(provider);
            }
            pool.Return(windowListSnapshot);
            TriggerResize();
            controller?.Unlock();
        }
        #endregion

        #region Static Methods
        internal static void NotifyThemeChanged(IThemeContext themeContext)
        {
            WeakReference<CoreWindow>[] windowListSnapshot = GetWindowListSnapshot(_rootWindowList, out ArrayPool<WeakReference<CoreWindow>> pool, out int count);
            for (int i = 0; i < count; i++)
            {
                if (!windowListSnapshot[i].TryGetTarget(out CoreWindow window) || window is null)
                    continue;
                D2D1DeviceContext deviceContext = window._deviceContext;
                if (deviceContext is null)
                    continue;
                window.ApplyTheme(new ThemeResourceProvider(deviceContext, themeContext, window._windowMaterial));
            }
            pool.Return(windowListSnapshot);
        }

        private static WeakReference<CoreWindow>[] GetWindowListSnapshot(List<WeakReference<CoreWindow>> windowList, out ArrayPool<WeakReference<CoreWindow>> pool,
            out int count)
        {
            lock (windowList)
            {
                count = windowList.Count;
                if (count <= 0)
                {
                    pool = null;
                    return null;
                }
                count -= windowList.RemoveAll(reference => !reference.TryGetTarget(out CoreWindow window) || window is null);
                if (count <= 0)
                {
                    pool = null;
                    return null;
                }
                pool = ArrayPool<WeakReference<CoreWindow>>.Shared;
                WeakReference<CoreWindow>[] windowListSnapshot = pool.Rent(count);
                windowList.CopyTo(0, windowListSnapshot, 0, count);
                return windowListSnapshot;
            }
        }
        #endregion

        #region Disposing
        protected override void Dispose(bool disposing)
        {
            DisposeElements(_overlayElementList.Unwrap());
            DisposeElements(_backgroundElementList.Unwrap());

            _controller?.Dispose();
            _titleLayout?.Dispose();

            SwapChainGraphicsHost host = _host;
            if (host is null)
            {
                base.Dispose(disposing);
                return;
            }
            base.Dispose(disposing);
        }

        protected static void DisposeElements(UIElement[][] elements_array)
        {
            if (elements_array is null) return;
            for (int i = 0, count = elements_array.Length; i < count; i++)
            {
                DisposeElements(elements_array[i]);
            }
        }

        [Inline]
        protected static void DisposeElements(UIElement[] elements)
        {
            if (elements is null) return;
            for (int i = 0, count = elements.Length; i < count; i++)
            {
                UIElement element = elements[i];
                if (element is IDisposable disposableElement)
                {
                    disposableElement.Dispose();
                }
            }
        }

        [Inline]
        protected static void DisposeElements(IList<UIElement> elements)
        {
            if (elements is null) return;
            for (int i = 0, count = elements.Count; i < count; i++)
            {
                UIElement element = elements[i];
                if (element is IDisposable disposableElement)
                {
                    disposableElement.Dispose();
                }
            }
        }
        #endregion
    }
}
