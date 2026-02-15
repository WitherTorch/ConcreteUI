using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Hosts;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Graphics.Native.DXGI;
using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Layout;
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
        private static readonly LazyTiny<GraphicsDeviceProvider> _graphicsDeviceProviderLazy
            = new LazyTiny<GraphicsDeviceProvider>(CreateGraphicsDeviceProvider, LazyThreadSafetyMode.ExecutionAndPublication);
        #endregion

        #region Fields
        private readonly ConcurrentDictionary<Type, UIElement> _overlayElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly ConcurrentDictionary<Type, UIElement> _backgroundElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly UnwrappableList<UIElement> _overlayElementList = new UnwrappableList<UIElement>();
        private readonly UnwrappableList<UIElement> _backgroundElementList = new UnwrappableList<UIElement>();
        private readonly WindowMaterial _windowMaterial;
        private SimpleGraphicsHost? _host;
        private DirtyAreaCollector? _collector;
        private RenderingController? _controller;
        private UIElement? _focusElement;
        private IThemeResourceProvider? _resourceProvider;
        private bool _isShown;
        private long _updateFlags = Booleans.TrueLong;
        #endregion

        #region Rendering Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private D2D1DeviceContext? _deviceContext;
        private DWriteTextLayout? _titleLayout;
        protected D2D1ColorF _clearDCColor, _windowBaseColor;
        protected RectF _minRect, _maxRect, _closeRect, _pageRect, _titleBarRect;
        protected BitVector64 _titleBarButtonStatus, _titleBarButtonChangedStatus;
        protected float _drawingOffsetX, _drawingOffsetY, _borderWidthInPointsX, _borderWidthInPointsY;
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
            SimpleGraphicsHost host;
            if (parent is null)
            {
                GraphicsDeviceProvider provider = _graphicsDeviceProviderLazy.Value;
                bool useFlipModel, useDComp;
                if (material == WindowMaterial.None && SystemConstants.VersionLevel >= SystemVersionLevel.Windows_8)
                {
                    useFlipModel = true;
                    useDComp = false;
                }
                else
                {
                    useFlipModel = useDComp = provider.IsSupportDComp && provider.IsSupportSwapChain1;
                }
                host = GraphicsHostHelper.CreateSwapChainGraphicsHost(handle, provider, useFlipModel, useDComp, IsBackgroundOpaque());
            }
            else
                host = GraphicsHostHelper.FromAnotherSwapChainGraphicsHost(parent._host!, handle, IsBackgroundOpaque());
            _host = host;
            _collector = new DirtyAreaCollector(host);
            D2D1DeviceContext? deviceContext = host.BeginDraw();
            if (deviceContext is null)
                return;
            (uint dpiX, uint dpiY) = Dpi;
            if (dpiX != SystemConstants.DefaultDpiX || dpiY != SystemConstants.DefaultDpiY)
                deviceContext.Dpi = new PointF(dpiX, dpiY);
            _deviceContext = deviceContext;

            ChangeBackgroundElement(new ToolTip(this, element => GetActiveElements().Contains(element)));
            InitializeElements();
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
            Point point = PointToClient(MouseHelper.GetMousePosition());
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
        }
        #endregion

        #region Implements Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GraphicsDeviceProvider GetGraphicsDeviceProvider() => _host!.GetDeviceProvider();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DXGISwapChain GetSwapChain() => _host!.GetSwapChain();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1DeviceContext GetDeviceContext() => NullSafetyHelper.ThrowIfNull(_deviceContext);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderingController? GetRenderingController() => _controller;

        public virtual void RenderBackground(UIElement element, in RegionalRenderingContext context)
            => context.Clear(_windowBaseColor);

        void IRenderingControl.Render(RenderingFlags flags)
        {
            bool resized = (flags & RenderingFlags.Resize) == RenderingFlags.Resize,
                resizedTemporarily = (flags & RenderingFlags.ResizeTemporarily) == RenderingFlags.ResizeTemporarily,
                redrawAll = (flags & RenderingFlags.RedrawAll) == RenderingFlags.RedrawAll;
            redrawAll = !RenderCore(redrawAll, resized, resizedTemporarily);
            if (redrawAll)
                _controller?.RequestUpdate(true);
        }

        ToolTip? IRenderer.GetToolTip() => GetBackgroundElement<ToolTip>();

        public IThemeResourceProvider? GetThemeResourceProvider() => InterlockedHelper.Read(ref _resourceProvider);

        public Vector2 GetPointsPerPixel() => _pointsPerPixel;

        IEnumerable<UIElement?> IElementContainer.GetActiveElements() => GetActiveElements();

        Point IElementContainer.PointToGlobal(Point point) => point;

        PointF IElementContainer.PointToGlobal(PointF point) => point;

        bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

        private bool IsBackgroundOpaque() => _windowMaterial == WindowMaterial.None;
        #endregion

        #region Abstract Methods
        protected abstract void InitializeElements();

        protected abstract IEnumerable<UIElement?> GetActiveElements();
        #endregion

        #region Virtual Methods
        public virtual IEnumerable<UIElement?> GetElements() => GetActiveElements()
            .ConcatOptimized(GetOverlayElements())
            .ConcatOptimized(GetBackgroundElements());

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
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseDownForElements(elements, ref args);
            UIElementHelper.OnMouseDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnMouseMoveForElements(in MouseNotifyEventArgs args)
        {
            SystemCursorType? cursorType = null;
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseMoveForElements(elements, args, ref cursorType);
            UIElementHelper.OnMouseMoveForElements(GetBackgroundElements(), args, ref cursorType);
            Cursor = SystemCursors.GetSystemCursor(cursorType.GetValueOrDefault(SystemCursorType.Default));
        }

        protected virtual void OnMouseUpForElements(in MouseNotifyEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseUpForElements(elements, args);
            UIElementHelper.OnMouseUpForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnMouseScrollForElements(ref MouseInteractEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseScrollForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnMouseScrollForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyDownForElements(ref KeyInteractEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnKeyDownForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyUpForElements(ref KeyInteractEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnKeyUpForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyUpForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnCharacterInputForElements(ref CharacterInteractEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnCharacterInputForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnCharacterInputForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnDpiChangedForElements(in DpiChangedEventArgs args)
        {
            UIElementHelper.OnDpiChangedForElements(GetOverlayElements(), in args);
            UIElementHelper.OnDpiChangedForElements(GetActiveElements(), in args);
            UIElementHelper.OnDpiChangedForElements(GetBackgroundElements(), in args);
        }

        protected virtual unsafe void RecalculateLayout(in SizeF windowSize, bool callRecalculatePageLayout)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
            {
                _pageRect = RenderingHelper.CeilingInPixel(RectF.FromXYWH(PointF.Empty, ClientSize), _pointsPerPixel);
            }
            else
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                Vector2 pixelsPerPoint = _pixelsPerPoint;
                float borderWidthInPointsX, borderWidthInPointsY;
                float drawingOffsetX, drawingOffsetY;
                if (User32.IsZoomed(handle))
                {
                    Rect windowRect;
                    if (!User32.GetWindowRect(handle, &windowRect))
                        Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                    if (!Screen.TryGetScreenInfoFromHwnd(handle, out ScreenInfo screenInfo))
                        screenInfo = default;
                    Rect workingArea = screenInfo.WorkingArea;
                    drawingOffsetX = (workingArea.Left - windowRect.Left) * pixelsPerPoint.X;
                    drawingOffsetY = (workingArea.Top - windowRect.Top) * pixelsPerPoint.Y;
                    borderWidthInPointsX = _borderWidthInPointsX = 0;
                    borderWidthInPointsY = _borderWidthInPointsY = 0;
                }
                else
                {
                    Vector2 pointsPerPixel = _pointsPerPixel;
                    float borderWidthInPixels = _borderWidthInPixels;
                    borderWidthInPointsX = borderWidthInPixels * RenderingHelper.GetDefaultBorderWidth(pointsPerPixel.X);
                    borderWidthInPointsY = borderWidthInPixels * RenderingHelper.GetDefaultBorderWidth(pointsPerPixel.Y);
                    _borderWidthInPointsX = borderWidthInPointsX;
                    _borderWidthInPointsY = borderWidthInPointsY;
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
                _pageRect = RenderingHelper.CeilingInPixel(new RectF(
                    left: drawingOffsetX + borderWidthInPointsX,
                    top: titleBarRect.Bottom + 1,
                    right: windowSize.Width - drawingOffsetX - borderWidthInPointsX,
                    bottom: windowSize.Height - borderWidthInPointsY), _pointsPerPixel);
            }
            if (callRecalculatePageLayout && _pageRect.IsValid)
                RecalculatePageLayout(_pageRect);
        }

        protected virtual void RecalculatePageLayout(in RectF pageRect)
        {
            Rect flooredPageRect = (Rect)pageRect;
            LayoutEngine layoutEngine = RentLayoutEngine();
            layoutEngine.RecalculateLayout(flooredPageRect, GetActiveElements());
            layoutEngine.RecalculateLayout(flooredPageRect, GetOverlayElements());
            ReturnLayoutEngine(layoutEngine);
        }
        #endregion

        #region Rendering
        protected IEnumerable<UIElement> GetOverlayElements() => _overlayElementList;

        protected IEnumerable<UIElement> GetBackgroundElements() => _backgroundElementList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected D2D1Brush GetBrush(Brush brush) => _brushes[(int)brush];

        [Inline]
        private bool RenderCore(bool force, bool resized, bool resizedTemporarily)
        {
            SimpleGraphicsHost? host = _host;
            if (host is null || host.IsDisposed)
                return false;
            DirtyAreaCollector? collector = _collector;
            if (collector is null)
                return false;
            if (resized)
            {
                force = true;
                Size size = RawClientSize;
                if (resizedTemporarily)
                    host.ResizeTemporarily(size);
                else
                    host.Resize(size);
                RecalculateLayout(GraphicsUtils.ScalingSize(size, _pixelsPerPoint), true);
            }
            D2D1DeviceContext? deviceContext = host.GetDeviceContext();
            if (deviceContext is null || deviceContext.IsDisposed)
                return true;

            ClearTypeSwitcher.SetClearType(deviceContext, false);
            if (force)
                return RenderCore_Force(host, deviceContext);
            else
                return RenderCore_Normal(host, deviceContext, collector);
        }

        private bool RenderCore_Force(SimpleGraphicsHost host, D2D1DeviceContext deviceContext)
        {
            DirtyAreaCollector collector = DirtyAreaCollector.Empty;
            RenderTitle(deviceContext, collector, force: true);
            RectF pageRect = _pageRect;
            if (pageRect.IsValid)
                RenderPage(deviceContext, collector, pageRect, force: true);
            host.Flush();

            if (ConcreteSettings.UseDebugMode)
            {
                host.Present();
                return true;
            }
            return host.TryPresent();
        }

        private bool RenderCore_Normal(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, DirtyAreaCollector collector)
        {
            Vector2 pointsPerPixel = _pointsPerPixel;

            RenderTitle(deviceContext, collector, force: false);
            RectF pageRect = _pageRect;
            if (pageRect.IsValid)
                RenderPage(deviceContext, collector, pageRect, force: false);
            host.Flush();

            if (ConcreteSettings.UseDebugMode)
            {
                collector.Present(pointsPerPixel);
                return true;
            }
            return collector.TryPresent(pointsPerPixel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect)
            => deviceContext.Clear(_windowBaseColor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderOnceContent(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPage(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect, bool force)
        {
            using RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, pageRect, D2D1AntialiasMode.Aliased);
            if (force)
            {
                collector.UsePresentAllModeOnce();
                RenderPageBackground(deviceContext, collector, pageRect);
                RenderOnceContent(deviceContext, collector, pageRect);
            }
            Vector2 pointPerPixel = _pointsPerPixel;
            UIElementHelper.RenderElements(deviceContext, collector, pointPerPixel, GetActiveElements(), ignoreNeedRefresh: force);
            UIElementHelper.RenderElements(deviceContext, collector, pointPerPixel, GetOverlayElements(), ignoreNeedRefresh: force || collector.HasAnyDirtyArea());
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
            BitVector64 titleBarStates = _titleBarStates;
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
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            TriggerResizeCore(controller, _sizeModeState);
        }

        private void UpdateFirstTime()
        {
            _isShown = true;
            RenderingController controller = new RenderingController(this, GetWindowFps(Handle));
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
        private void ChangeDpi_RenderingPart(PointU dpi, Vector2 pointsPerPixel, Vector2 pixelsPerPoint)
        {
            SimpleGraphicsHost? host = _host;
            if (host is null || host.IsDisposed)
                return;
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            controller.Lock();
            controller.WaitForRendering();
            host.GetDeviceContext().Dpi = new PointF(dpi.X, dpi.Y);
            OnDpiChangedForElements(new DpiChangedEventArgs(dpi, pointsPerPixel, pixelsPerPoint));
            controller.Unlock();
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
        protected void TriggerResize()
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            TriggerResizeCore(controller, _sizeModeState);
        }

        private static void TriggerResizeCore(RenderingController controller, uint sizeModeState) => controller.RequestResize(sizeModeState == 2u);

        protected void Update()
        {
            if (!_isShown)
                return;
            UpdateCore(_controller);
        }

        protected void Refresh()
        {
            if (!_isShown)
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
            if (!items.HasAnyItem())
                return;

            ContextMenu contextMenu = new ContextMenu(this, items);
            ChangeOverlayElement(contextMenu)?.Dispose();
            RectF pageRect = _pageRect;
            if (location.X + contextMenu.Width >= pageRect.Right)
                location.X = location.X - contextMenu.Width + 1;
            if (location.Y + contextMenu.Height >= pageRect.Bottom)
                location.Y = location.Y - contextMenu.Height + 1;
            contextMenu.Location = location;
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
                TriggerResizeCore(controller, _sizeModeState);
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
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider);
                DisposeHelper.SwapDisposeInterlocked(ref _controller);
                SimpleGraphicsHost? host = InterlockedHelper.Exchange(ref _host, null);
                if (host is not null && !host.IsDisposed)
                {
                    host.EndDraw();
                    host.Dispose();
                }
                DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
                DisposeHelper.DisposeAll(_brushes);
                DisposeElements(GetElements());
            }
            _overlayElementList.Clear();
            _backgroundElementList.Clear();
            SequenceHelper.Clear(_brushes);
            base.DisposeCore(disposing);
        }

        private static void DisposeElements(IEnumerable<UIElement?> elements)
        {
            switch (elements)
            {
                case UIElement?[] array:
                    DisposeElementsCore(array, array.Length);
                    return;
                case IList<UIElement?> list:
                    DisposeElementsCore(list);
                    return;
                case ICollection<UIElement?> collection:
                    DisposeElementsCore(collection);
                    return;
                default:
                    DisposeElementsCore(elements);
                    return;
            }
        }

        private static void DisposeElementsCore(UIElement?[] elements, int length)
        {
            if (length <= 0)
                return;
            ref UIElement? elementRef = ref elements[0];
            for (int i = 0; i < length; i++)
                (UnsafeHelper.AddTypedOffset(ref elementRef, i) as IDisposable)?.Dispose();
        }

        private static void DisposeElementsCore(IList<UIElement?> elements)
        {
            switch (elements)
            {
                case UnwrappableList<UIElement?> list:
                    DisposeElementsCore(list.Unwrap(), list.Count);
                    return;
                case ObservableList<UIElement?> list:
                    {
                        IList<UIElement?> underlyingList = list.GetUnderlyingList();
                        if (ReferenceEquals(underlyingList, list))
                            return;
                        DisposeElementsCore(list);
                    }
                    return;
                default:
                    {
                        int count = elements.Count;
                        if (count <= 0)
                            return;
                        for (int i = 0; i < count; i++)
                            (elements[i] as IDisposable)?.Dispose();
                    }
                    return;
            }
        }

        private static void DisposeElementsCore(ICollection<UIElement?> elements)
        {
            int count = elements.Count;
            if (count <= 0)
                return;
            using IEnumerator<UIElement?> enumerator = elements.GetEnumerator();
            enumerator.MoveNext();
            for (int i = 0; i < count; i++)
            {
                (enumerator.Current as IDisposable)?.Dispose();
                if (!enumerator.MoveNext())
                    break;
            }
        }

        private static void DisposeElementsCore(IEnumerable<UIElement?> elements)
        {
            foreach (UIElement? element in elements)
                (element as IDisposable)?.Dispose();
        }
        #endregion
    }
}
