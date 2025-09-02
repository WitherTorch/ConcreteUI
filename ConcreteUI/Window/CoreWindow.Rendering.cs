using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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
using ConcreteUI.Layout;
using ConcreteUI.Native;
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
            "back",
            "fore.active",
            "fore.deactive",
            "closeButton.active",
        }.WithPrefix("app.title.").ToLowerAscii();
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
        private SwapChainGraphicsHost? _host;
        private DirtyAreaCollector? _collector;
        private RenderingController? _controller;
        private UIElement? _focusElement;
        private IThemeResourceProvider? _resourceProvider;
        private float baseLineWidth = 1.0f;
        private bool isShown, isInitializingElements;
        private long _updateFlags = Booleans.TrueLong;
        #endregion

        #region Rendering Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private D2D1DeviceContext? _deviceContext;
        private DWriteTextLayout? _titleLayout;
        protected D2D1ColorF _clearDCColor, _windowBaseColor;
        protected RectF _minRect, _maxRect, _closeRect, _pageRect, _titleBarRect;
        protected BitVector64 _titleBarButtonStatus, _titleBarButtonChangedStatus;
        protected float _drawingOffsetX, _drawingOffsetY, _drawingBorderWidth;
        #endregion

        #region Properties
#pragma warning disable CS0109
        public new ContextMenu? ContextMenu => GetOverlayElement<ContextMenu>();
#pragma warning restore CS0109
        public ToolTip? ToolTip => GetBackgroundElement<ToolTip>();
        public UIElement? FocusedElement => _focusElement;
        public WindowMaterial WindowMaterial => _windowMaterial;
        #endregion

        #region Events
        public event EventHandler<ContextMenu>? ContextMenuChanging;

        public event EventHandler<UIElement?>? FocusElementChanged;
        #endregion

        #region Event Handlers
        protected virtual void OnContextMenuChanging(ContextMenu newContextMenu) => ContextMenuChanging?.Invoke(this, newContextMenu);
        #endregion

        #region Init
        private static GraphicsDeviceProvider CreateGraphicsDeviceProvider()
        {
            string targetGpuName = ConcreteSettings.TargetGpuName;
            bool isDebug = ConcreteSettings.UseDebugMode;
            if (StringHelper.IsNullOrEmpty(targetGpuName))
                return new GraphicsDeviceProvider(DXGIGpuPreference.Invalid, isDebug);
            if (targetGpuName.StartsWith('#'))
            {
                DXGIGpuPreference preference = targetGpuName switch
                {
                    ConcreteSettings.ReservedGpuName_Default => DXGIGpuPreference.Unspecified,
                    ConcreteSettings.ReservedGpuName_MinimumPower => DXGIGpuPreference.MinimumPower,
                    ConcreteSettings.ReservedGpuName_HighPerformance => DXGIGpuPreference.HighPerformance,
                    _ => DXGIGpuPreference.Invalid,
                };
                return new GraphicsDeviceProvider(preference, isDebug);
            }
            return new GraphicsDeviceProvider(targetGpuName, isDebug);
        }

        [Inline(InlineBehavior.Remove)]
        private void InitRenderObjects(IntPtr handle)
        {
            WindowMaterial material = _windowMaterial;
            CoreWindow? parent = _parent;
            SwapChainGraphicsHost host;
            if (parent is null)
                host = GraphicsHostHelper.CreateSwapChainGraphicsHost(handle, graphicsDeviceProviderLazy.Value,
                    useFlipModel: material == WindowMaterial.None && SystemConstants.VersionLevel >= SystemVersionLevel.Windows_8);
            else
                host = GraphicsHostHelper.FromAnotherSwapChainGraphicsHost(parent._host!, handle);
            _host = host;
            _collector = DirtyAreaCollector.TryCreate(host as SwapChainGraphicsHost1);
            D2D1DeviceContext? deviceContext = _host!.BeginDraw();
            if (deviceContext is null)
                return;
            uint dpi = Dpi;
            if (dpi != 96)
                deviceContext.Dpi = new PointF(dpi, dpi);
            _deviceContext = deviceContext;

            ChangeBackgroundElement(new ToolTip(this, element => GetRenderingElements().Contains(element)));
            isInitializingElements = true;
            InitializeElements();
            isInitializingElements = false;
            ApplyTheme(parent is null ?
                ThemeResourceProvider.CreateResourceProvider(deviceContext, ThemeManager.CurrentTheme, material) :
                parent._resourceProvider!.Clone());
            SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            ConcreteUtils.ApplyWindowStyle(this, out _fixLagObject);
        }
        #endregion

        #region Override Methods
        protected override void OnShown(EventArgs args)
        {
            base.OnShown(args);
            UpdateFirstTime();
            WindowMessageLoop.InvokeAsync(OnShown2);
        }

        private void OnShown2()
        {
            PointF point = PointToClient(MouseHelper.GetMousePosition());
            OnMouseMove(new MouseInteractEventArgs(point));
        }

        protected override void OnClosing(ref ClosingEventArgs args)
        {
            base.OnClosing(ref args);

            if (args.Cancelled)
                return;

            SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

            RenderingController? controller = _controller;
            if (controller is not null)
            {
                controller.Stop();
                controller.WaitForExit(500);
            }

            SwapChainGraphicsHost? host = _host;
            if (host is not null && !host.IsDisposed)
                host.EndDraw();
        }
        #endregion

        #region Implements Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GraphicsDeviceProvider GetGraphicsDeviceProvider() => _host!.GetDeviceProvider();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1DeviceContext GetDeviceContext() => NullSafetyHelper.ThrowIfNull(_deviceContext);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderingController? GetRenderingController() => _controller;

        public virtual void RenderElementBackground(UIElement element, D2D1DeviceContext context)
            => context.Clear(_windowBaseColor);

        void IRenderingControl.Render(RenderingFlags flags)
        {
            bool resized = (flags & RenderingFlags.Resize) == RenderingFlags.Resize,
                redrawAll = (flags & RenderingFlags.RedrawAll) == RenderingFlags.RedrawAll;
            redrawAll = !RenderCore(redrawAll, resized);
            if (redrawAll)
                _controller?.RequestUpdate(true);
        }

        ToolTip? IRenderer.GetToolTip() => GetBackgroundElement<ToolTip>();

        bool IRenderer.IsInitializingElements() => isInitializingElements;

        public IThemeResourceProvider? GetThemeResourceProvider() => InterlockedHelper.Read(ref _resourceProvider);

        public float GetBaseLineWidth() => baseLineWidth;
        #endregion

        #region Abstract Methods
        protected abstract void InitializeElements();
        protected abstract IEnumerable<UIElement> GetRenderingElements();
        #endregion

        #region Virtual Methods
        protected virtual void ApplyThemeCore(IThemeResourceProvider provider)
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

        protected virtual void OnMouseDownForElements(ref MouseInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseDownForElements(elements, ref args);
            UIElementHelper.OnMouseDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnMouseMoveForElements(in MouseNotifyEventArgs args)
        {
            SystemCursorType? cursorType = null;
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseMoveForElements(elements, args, ref cursorType);
            UIElementHelper.OnMouseMoveForElements(GetBackgroundElements(), args, ref cursorType);
            Cursor = SystemCursors.GetSystemCursor(cursorType.GetValueOrDefault(SystemCursorType.Default));
        }

        protected virtual void OnMouseUpForElements(in MouseNotifyEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseUpForElements(elements, args);
            UIElementHelper.OnMouseUpForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnMouseScrollForElements(ref MouseInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnMouseScrollForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnMouseScrollForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyDownForElements(ref KeyInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnKeyDownForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyUpForElements(ref KeyInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnKeyUpForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyUpForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnCharacterInputForElements(ref CharacterInteractEventArgs args)
        {
            IEnumerable<UIElement> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetRenderingElements();
            UIElementHelper.OnCharacterInputForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnCharacterInputForElements(GetBackgroundElements(), ref args);
        }

        protected virtual unsafe void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
            {
                SizeF size = ClientSize;
                _pageRect = GraphicsUtils.AdjustRectangleF(RectF.FromXYWH(0, 0, size.Width, size.Height));
            }
            else
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                float windowScaleFactor = _windowScaleFactor;
                float drawingBorderWidth;
                float drawingOffsetX, drawingOffsetY;
                if (User32.IsZoomed(handle))
                {
                    Rect windowRect;
                    if (!User32.GetWindowRect(handle, &windowRect))
                        Marshal.ThrowExceptionForHR(User32.GetLastError());
                    if (!Screen.TryGetScreenInfoFromHwnd(handle, out ScreenInfo screenInfo))
                        screenInfo = default;
                    Rect workingArea = screenInfo.WorkingArea;
                    drawingOffsetX = (workingArea.Left - windowRect.Left) * windowScaleFactor;
                    drawingOffsetY = (workingArea.Top - windowRect.Top) * windowScaleFactor;
                    drawingBorderWidth = _drawingBorderWidth = 0;
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
                _closeRect = RectF.FromXYWH(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                _maxRect = RectF.FromXYWH(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                _minRect = RectF.FromXYWH(x - UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                RectF titleBarRect = _titleBarRect = RectF.FromXYWH(drawingOffsetX + 1, drawingOffsetY + 1, Size.Width - 2, 26);
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
            SwapChainGraphicsHost? host = _host;
            if (resized)
            {
                if (host is null || host.IsDisposed)
                    return false;
                isSizeChanged = force = true;
                Size size = base.ClientSize;
                host.Resize(size);
                RecalculateLayout(ScalingSizeF(size, _windowScaleFactor), true);
            }
            D2D1DeviceContext? deviceContext = host?.GetDeviceContext();
            if (deviceContext is null || deviceContext.IsDisposed)
                return true;
            DirtyAreaCollector? rawCollector = force ? null : _collector;
            DirtyAreaCollector collector = rawCollector ?? DirtyAreaCollector.Empty;
            RenderTitle(deviceContext, collector, force);
            RectF pageRect = _pageRect;
            if (!pageRect.IsValid)
                return true;
            RenderPage(deviceContext, collector, pageRect, force);
            host!.Flush();
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
                rawCollector.Present(_dpiScaleFactor);
                return true;
            }
            return rawCollector.TryPresent(_dpiScaleFactor);
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
                DWriteTextLayout? titleLayout = Interlocked.Exchange(ref _titleLayout, null);
                if (titleLayout is null || (Interlocked.Exchange(ref _updateFlags, Booleans.FalseLong) & (long)UpdateFlags.ChangeTitle) == (long)UpdateFlags.ChangeTitle)
                {
                    DWriteFactory factory = SharedResources.DWriteFactory;
                    DWriteTextFormat? titleFormat = titleLayout;
                    if (titleFormat is null || titleFormat.IsDisposed)
                    {
                        titleFormat = factory.CreateTextFormat(_resourceProvider!.FontName, UIConstants.TitleFontSize);
                        titleFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
                    }
                    titleLayout = GraphicsUtils.CreateCustomTextLayout(Text, titleFormat, 26);
                    titleFormat.Dispose();
                }
                ClearDCForTitle(deviceContext);
                if (titleBarStates[0])
                {
                    deviceContext.PushAxisAlignedClip(_titleBarRect, D2D1AntialiasMode.Aliased);
                    deviceContext.DrawTextLayout(new PointF(_drawingOffsetX + 7.5f, _drawingOffsetY + 1.5f), titleLayout, brushes[(int)Brush.TitleForeBrush]);
                    deviceContext.PopAxisAlignedClip();
                }
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            }
            BitVector64 TitleBarButtonStatus = _titleBarButtonStatus;
            FontIconResources iconStorer = FontIconResources.Instance;
            if (HasSizableBorder)
            {
                if (titleBarStates[1] && (TitleBarButtonChangedStatus[0] || force))
                {
                    RectF minRect = _minRect;
                    deviceContext.PushAxisAlignedClip(minRect, D2D1AntialiasMode.Aliased);
                    if (!force)
                        ClearDCForTitle(deviceContext);
                    iconStorer.RenderMinimizeButton(deviceContext, (RectangleF)minRect,
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
                        iconStorer.RenderRestoreButton(deviceContext, (RectangleF)maxRect, foreBrush);
                    else
                        iconStorer.RenderMaximizeButton(deviceContext, (RectangleF)maxRect, foreBrush);
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
                iconStorer.RenderCloseButton(deviceContext, (RectangleF)closeRect,
                        TitleBarButtonStatus[2] ? brushes[(int)Brush.TitleCloseButtonActiveBrush] : brushes[(int)Brush.TitleForeDeactiveBrush]);
                deviceContext.PopAxisAlignedClip();
                collector.MarkAsDirty(closeRect);
            }
            #endregion
        }
        #endregion

        #region Event Handlers
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e) => _controller?.Unlock();

        private void SystemEvents_DisplaySettingsChanging(object? sender, EventArgs e) => _controller?.Lock();

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    _controller?.Lock();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    _controller?.Unlock();
                    break;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    _controller?.Lock();
                    break;
                case PowerModes.Resume:
                    _controller?.Unlock();
                    break;
            }
        }
        #endregion

        #region Public Methods

        private void ResetBlur()
        {
            ConcreteUtils.ResetBlur(this);
        }

        protected override void OnResized(EventArgs args)
        {
            base.OnResized(args);
            _controller?.RequestResize();
        }

        private void UpdateFirstTime()
        {
            isShown = true;
            RenderingController controller = new RenderingController(this);
            _controller = controller;
            UpdateCoreUnchecked(controller);
        }

        [Inline(InlineBehavior.Remove)]
        private static void UpdateCoreUnchecked(RenderingController controller) => controller.RequestUpdate(true);

        [Inline(InlineBehavior.Remove)]
        private static void UpdateCore(RenderingController? controller)
        {
            if (controller is null)
                return;
            UpdateCoreUnchecked(controller);
        }

        [Inline(InlineBehavior.Remove)]
        private void OnDpiChangedRenderingPart()
        {
            SwapChainGraphicsHost? host = _host;
            if (host is null || host.IsDisposed)
                return;
            D2D1DeviceContext context = host.GetDeviceContext();
            uint dpi = Dpi;
            context.Dpi = new PointF(dpi, dpi);
            UpdateCore(_controller);
        }

        [Inline(InlineBehavior.Remove)]
        private void OnWindowStateChangedRenderingPart(in WindowStateChangedEventArgs args)
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            switch (args.NewState)
            {
                case WindowState.Maximized:
                    {
                        controller.RequestUpdate(true);
                        if (args.OldState == WindowState.Minimized)
                            controller.Unlock();
                    }
                    break;
                case WindowState.Normal:
                    {
                        controller.RequestUpdate(true);
                        if (args.OldState == WindowState.Minimized)
                            controller.Unlock();
                    }
                    break;
                case WindowState.Minimized:
                    {
                        controller.Lock();
                    }
                    break;
            }
        }

        private void CloseContextMenu(object? sender, EventArgs e) => ChangeOverlayElement(typeof(ContextMenu), null);
        #endregion

        #region Normal Methods
        protected void TriggerResize() => _controller?.RequestResize();

        protected void Update()
        {
            if (!isShown)
                return;
            UpdateCore(_controller);
        }

        protected void Refresh()
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

        protected T? GetOverlayElement<T>() where T : UIElement => GetOverlayElement(typeof(T)) as T;

        protected UIElement? GetOverlayElement(Type type) => _overlayElementDict.GetOrDefault(type, null);

        protected T? ChangeOverlayElement<T>(T element, Predicate<T>? predicate = null) where T : UIElement
        {
            Predicate<UIElement>? translatedPredicate;
            if (predicate is null)
                translatedPredicate = null;
            else
                translatedPredicate = obj => obj is not T castedObj || predicate(castedObj);
            return ChangeOverlayElement(typeof(T), element, translatedPredicate) as T;
        }

        protected UIElement? ChangeOverlayElement(Type type, UIElement? element, Predicate<UIElement>? predicate = null)
        {
            ConcurrentDictionary<Type, UIElement> overlayElementDict = _overlayElementDict;
            UnwrappableList<UIElement> overlayElementList = _overlayElementList;
            UIElement? result;
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
                IThemeResourceProvider? resourceProvider = _resourceProvider;
                if (resourceProvider is not null)
                    element.ApplyTheme(resourceProvider);

                LayoutEngine layoutEngine = RentLayoutEngine();
                layoutEngine.RecalculateLayout((Rect)_pageRect, element);
                ReturnLayoutEngine(layoutEngine);

                overlayElementDict.TryAdd(type, element);
                overlayElementList.Add(element);

                Refresh();
                return null;
            }
            if (result is null || predicate is null || predicate.Invoke(result))
            {
                IThemeResourceProvider? resourceProvider = _resourceProvider;
                if (resourceProvider is not null)
                    element.ApplyTheme(resourceProvider);

                LayoutEngine layoutEngine = RentLayoutEngine();
                layoutEngine.RecalculateLayout((Rect)_pageRect, element);
                ReturnLayoutEngine(layoutEngine);

                int index = result is null ? -1 : overlayElementList.IndexOf(result);
                overlayElementDict[type] = element;
                if (index > -1)
                    overlayElementList[index] = element;
                else
                    overlayElementList.Add(element);

                Refresh();
                return result;
            }
            return null;
        }

        protected T? GetBackgroundElement<T>() where T : UIElement => GetBackgroundElement(typeof(T)) as T;

        protected UIElement? GetBackgroundElement(Type type) => _backgroundElementDict.GetOrDefault(type, null);

        protected T? ChangeBackgroundElement<T>(T element, Predicate<T>? predicate = null) where T : UIElement
        {
            Predicate<UIElement>? translatedPredicate;
            if (predicate is null)
                translatedPredicate = null;
            else
                translatedPredicate = obj => obj is not T castedObj || predicate(castedObj);
            return ChangeBackgroundElement(typeof(T), element, translatedPredicate) as T;
        }

        protected UIElement? ChangeBackgroundElement(Type type, UIElement? element, Predicate<UIElement>? predicate = null)
        {
            ConcurrentDictionary<Type, UIElement> backgroundElementDict = _backgroundElementDict;
            UnwrappableList<UIElement> backgroundElementList = _backgroundElementList;
            UIElement? result;
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
                return null;
            }
            if (result is null || predicate is null || predicate.Invoke(result))
            {
                int index = result is null ? -1 : backgroundElementList.IndexOf(result);
                backgroundElementDict[type] = element;
                if (index > -1)
                    backgroundElementList[index] = element;
                else
                    backgroundElementList.Add(element);
                return result;
            }
            return null;
        }

        public void OpenContextMenu(ContextMenu.ContextMenuItem[] items, Point location)
        {
            if (items.HasAnyItem())
            {
                ContextMenu contextMenu = new ContextMenu(this, items)
                {
                    Location = location
                };
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

        [Inline(InlineBehavior.Keep, export: true)]
        public void CloseOverlayElement(UIElement elementForValidate)
            => CloseOverlayElement(elementForValidate.GetType(), elementForValidate);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseOverlayElement(Type elementType, UIElement elementForValidate)
            => (ChangeOverlayElement(elementType, null, _element => Equals(_element, elementForValidate)) as IDisposable)?.Dispose();

        protected void ApplyTheme(IThemeResourceProvider provider)
        {
            RenderingController? controller = _controller;
            if (controller is not null)
            {
                controller.Lock();
                controller.WaitForRendering();
            }
            DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider, provider);
            ApplyThemeCore(provider);
            ConcreteUtils.ResetBlur(this);
            if (TryGetWindowListSnapshot(_childrenReferenceList, out ArrayPool<WeakReference<CoreWindow>>? pool,
                    out WeakReference<CoreWindow>[]? array, out int count))
            {
                for (int i = 0; i < count; i++)
                {
                    if (!array[i].TryGetTarget(out CoreWindow? window) || window is null || window.IsDisposed)
                        continue;
                    D2D1DeviceContext? deviceContext = window._deviceContext;
                    if (deviceContext is null || deviceContext.IsDisposed)
                        continue;
                    window.ApplyTheme(provider.Clone());
                }
                pool.Return(array);
            }
            if (controller is not null)
            {
                controller.RequestResize();
                controller.Unlock();
            }
        }
        #endregion

        #region Static Methods
        internal static void NotifyThemeChanged(IThemeContext themeContext)
        {
            if (!TryGetWindowListSnapshot(_rootWindowList, out ArrayPool<WeakReference<CoreWindow>>? pool,
                    out WeakReference<CoreWindow>[]? array, out int count))
                return;
            for (int i = 0; i < count; i++)
            {
                if (!array[i].TryGetTarget(out CoreWindow? window) || window is null || window.IsDisposed)
                    continue;
                D2D1DeviceContext? deviceContext = window._deviceContext;
                if (deviceContext is null || deviceContext.IsDisposed)
                    continue;
                window.ApplyTheme(ThemeResourceProvider.CreateResourceProvider(window, themeContext));
            }
            pool.Return(array);
        }

        private static bool TryGetWindowListSnapshot(List<WeakReference<CoreWindow>> windowList,
            [NotNullWhen(true)] out ArrayPool<WeakReference<CoreWindow>>? pool, [NotNullWhen(true)] out WeakReference<CoreWindow>[]? array, out int count)
        {
            lock (windowList)
            {
                count = windowList.Count;
                if (count <= 0)
                {
                    pool = null;
                    array = null;
                    return false;
                }
                count -= windowList.RemoveAll(reference => !reference.TryGetTarget(out CoreWindow? window) || window is null);
                if (count <= 0)
                {
                    pool = null;
                    array = null;
                    return false;
                }
                pool = ArrayPool<WeakReference<CoreWindow>>.Shared;
                array = pool.Rent(count);
                windowList.CopyTo(0, array, 0, count);
                return true;
            }
        }
        #endregion

        #region Disposing
        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider);
                DisposeHelper.SwapDisposeInterlocked(ref _controller);
                DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
                DisposeHelper.SwapDisposeInterlocked(ref _host);
                DisposeHelper.DisposeAllWeak(_overlayElementList.Unwrap());
                DisposeHelper.DisposeAllWeak(_backgroundElementList.Unwrap());
                DisposeHelper.DisposeAll(_brushes);
            }
            _overlayElementList.Clear();
            _backgroundElementList.Clear();
            SequenceHelper.Clear(_brushes);
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
