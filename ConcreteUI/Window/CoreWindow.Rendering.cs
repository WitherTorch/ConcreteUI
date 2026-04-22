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
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

using ContextMenu = ConcreteUI.Controls.ContextMenu;
using ToolTip = ConcreteUI.Controls.ToolTip;

namespace ConcreteUI.Window
{
    public abstract partial class CoreWindow : IRenderer, IElementContainer
    {
        #region Enums
        [Flags]
        private enum UpdateFlags : long
        {
            None = 0,
            ChangeTitle = 0b1,
        }

        protected enum Brush : uint
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

        #endregion

        #region Fields
        private readonly ConcurrentDictionary<Type, UIElement> _overlayElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly ConcurrentDictionary<Type, UIElement> _backgroundElementDict = new ConcurrentDictionary<Type, UIElement>();
        private readonly UnwrappableList<UIElement> _overlayElementList = new UnwrappableList<UIElement>();
        private readonly UnwrappableList<UIElement> _backgroundElementList = new UnwrappableList<UIElement>();
        private readonly LazyTiny<WeakReference> _focusElementRefLazy, _lastHitElementRefLazy, _recordedLastHitElementRefLazy;
        private readonly WindowMaterial _windowMaterial;
        private SimpleGraphicsHost? _host;
        private DirtyAreaCollector? _collector;
        private RenderingController? _controller;
        private IThemeResourceProvider? _resourceProvider;
        private bool _isShown;
        private long _updateFlags = Booleans.TrueLong;
        #endregion

        #region Rendering Fields
        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private GraphicsDeviceProvider? _graphicsDeviceProvider;
        private D2D1DeviceContext? _deviceContext;
        private DWriteTextLayout? _titleLayout;
        private nuint _ownedGDP, _recreateGraphicsDeviceProviderBarrier;

        protected D2D1ColorF _clearDCColor, _windowBaseColor;
        protected Rectangle _minRect, _maxRect, _closeRect;
        protected Rect _pageRect, _titleBarRect;
        protected BitVector64 _titleBarButtonStatus, _titleBarButtonChangedStatus;
        protected int _drawingOffsetX, _drawingOffsetY, _activeBorderWidth;
        #endregion

        #region Static Properties
        public static LayoutVariable PageLeftReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Left);
        }

        public static LayoutVariable PageTopReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Top);
        }

        public static LayoutVariable PageRightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Right);
        }

        public static LayoutVariable PageBottomReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Bottom);
        }

        public static LayoutVariable PageWidthReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Width);
        }

        public static LayoutVariable PageHeightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LayoutVariable.PageReference(LayoutProperty.Height);
        }
        #endregion

        #region Properties
        public ContextMenu? ContextMenu => GetOverlayElement<ContextMenu>();

        public ToolTip? ToolTip => GetBackgroundElement<ToolTip>();

        public UIElement? FocusedElement
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _focusElementRefLazy.GetValueDirectly()?.Target as UIElement;
        }

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
            if (!InitRenderObjectsCore(handle, GetGraphicsDeviceProvider(), out D2D1DeviceContext? deviceContext))
                return;
            _deviceContext = deviceContext;

            CoreWindow? parent = _parent;
            ChangeBackgroundElement(new ToolTip(this, element => GetActiveElements().Contains(element)));
            InitializeElements();
            ApplyTheme(parent is null ?
                ThemeResourceProvider.CreateResourceProvider(deviceContext, ThemeManager.CurrentTheme, _windowMaterial) :
                parent._resourceProvider!.Clone());
            if (parent is null)
                ApplyTheme(ThemeResourceProvider.CreateResourceProvider(deviceContext, ThemeManager.CurrentTheme, _windowMaterial));
            else
                ApplyTheme(parent._resourceProvider!.Clone());
            SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            ConcreteUtils.ApplyWindowStyle(this, out _fixLagObject);
        }

        private bool InitRenderObjectsCore(IntPtr handle, GraphicsDeviceProvider provider, [NotNullWhen(true)] out D2D1DeviceContext? deviceContext)
        {
            if (handle == IntPtr.Zero)
            {
                deviceContext = null;
                return false;
            }

            WindowMaterial material = _windowMaterial;
            CoreWindow? parent = _parent;

            SimpleGraphicsHost host;
            SystemVersionLevel versionLevel = SystemConstants.VersionLevel;
            bool useFlipModel = ExtendedStyles.HasFlagFast(WindowExtendedStyles.NoRedirectionBitmap);
            bool useDComp = useFlipModel && provider.IsSupportDComp && provider.IsSupportSwapChain1;
            host = GraphicsHostHelper.CreateSwapChainGraphicsHost(handle, provider, useFlipModel, useDComp, IsBackgroundOpaque());
            _host = host;
            _collector = new DirtyAreaCollector(host);
            if (parent is null)
                host.DeviceRemoved += GraphicsHost_DeviceRemoved;
            deviceContext = host.GetDeviceContext();
            if (deviceContext is null)
                return false;
            (uint dpiX, uint dpiY) = Dpi;
            if (dpiX != SystemConstants.DefaultDpiX || dpiY != SystemConstants.DefaultDpiY)
                deviceContext.Dpi = new PointF(dpiX, dpiY);
            return true;
        }

        private void GraphicsHost_DeviceRemoved(object? sender, EventArgs e)
        {
            if (sender is not SimpleGraphicsHost host || !ReferenceEquals(host, _host))
                return;
            WindowMessageLoop.InvokeAsync((Action<CoreWindow>)(static window => window.OnDeviveRemoved()), this);
        }

        private void OnDeviveRemoved()
        {
            if (InterlockedHelper.Exchange(ref _recreateGraphicsDeviceProviderBarrier, UnsafeHelper.GetMaxValue<nuint>()) != 0)
                return;

            GraphicsDeviceProvider? collectionTarget = InterlockedHelper.Read(ref _graphicsDeviceProvider);
            if (collectionTarget is not null)
            {
                StopAllRenderingFromGDREvent();
                GC.Collect(GC.GetGeneration(collectionTarget), GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
                RecreateResourcesFromGDREvent(null, null);
            }
            else
            {
                StopAllRenderingFromGDREvent();
                RecreateResourcesFromGDREvent(null, null);
            }

            InterlockedHelper.Exchange(ref _recreateGraphicsDeviceProviderBarrier, 0);
        }

        private unsafe void StopAllRenderingFromGDREvent()
        {
            DebugHelper.WriteLine("GDR event triggered. Stopping all rendering...");

            RenderingController? controller = _controller;
            if (controller is null)
                return;
            controller.Lock();
            controller.WaitForRendering();
            DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider, ThemeResourceProvider.Empty);
            ApplyThemeCore(ThemeResourceProvider.Empty);
            DisposeHelper.SwapDisposeInterlocked(ref _host);
            InterlockedHelper.Exchange(ref _collector, null);
            InterlockedHelper.Exchange(ref _deviceContext, null);
            if (TryGetWindowListSnapshot(_childrenReferenceList, out NativeMemoryPool? pool,
                out TypedNativeMemoryBlock<GCHandle> handles, out int count))
            {
                try
                {
                    DebugHelper.ThrowIf(count <= 0);
                    GCHandle* ptr = handles.NativePointer;
                    for (int i = 0; i < count; i++)
                    {
                        GCHandle handle = ptr[i];
                        if (!handle.IsAllocated || handle.Target is not CoreWindow window || window.IsDisposed)
                            continue;
                        window.StopAllRenderingFromGDREvent();
                    }
                }
                finally
                {
                    pool.Return(handles);
                }
            }
        }

        private unsafe void RecreateResourcesFromGDREvent(GraphicsDeviceProvider? deviceProvider, IThemeResourceProvider? resourceProvider)
        {
            if (deviceProvider is null)
            {
                DebugHelper.WriteLine("Recreating GDP...");
                deviceProvider = CreateGraphicsDeviceProvider();
                InterlockedHelper.Write(ref _ownedGDP, UnsafeHelper.GetMaxValue<nuint>());
                DebugHelper.WriteLine("Recreated GDP...");
            }
            else
            {
                InterlockedHelper.Write(ref _ownedGDP, 0);
            }
            DisposeHelper.SwapDisposeInterlocked(ref _graphicsDeviceProvider, deviceProvider);

            DebugHelper.WriteLine("Recreating device context...");
            if (!InitRenderObjectsCore(Handle, deviceProvider, out D2D1DeviceContext? deviceContext))
            {
                DebugHelper.WriteLine("Failed to recreate device context in GDR event.");
                return;
            }
            RenderingController? controller = _controller;
            DebugHelper.ThrowIf(controller is null);
            _deviceContext = deviceContext;

            DebugHelper.WriteLine("Recreating resources...");
            resourceProvider ??= ThemeResourceProvider.CreateResourceProvider(this, ThemeManager.CurrentTheme);
            DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider, resourceProvider);
            ApplyThemeCore(resourceProvider);
            if (TryGetWindowListSnapshot(_childrenReferenceList, out NativeMemoryPool? pool,
                out TypedNativeMemoryBlock<GCHandle> handles, out int count))
            {
                try
                {
                    DebugHelper.ThrowIf(count <= 0);
                    GCHandle* ptr = handles.NativePointer;
                    for (int i = 0; i < count; i++)
                    {
                        GCHandle handle = ptr[i];
                        if (!handle.IsAllocated || handle.Target is not CoreWindow window || window.IsDisposed)
                            continue;
                        window.RecreateResourcesFromGDREvent(deviceProvider, resourceProvider);
                    }
                }
                finally
                {
                    pool.Return(handles);
                }
            }
            TriggerResizeCore(controller, _sizeModeState);
            controller.Unlock();
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
            OnMouseMove(new HandleableMouseEventArgs(point));
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
        public GraphicsDeviceProvider GetGraphicsDeviceProvider()
        {
            if (InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                SpinWait.SpinUntil(() => InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) == 0);
            GraphicsDeviceProvider? deviceProvider = InterlockedHelper.Read(ref _graphicsDeviceProvider);
            if (deviceProvider is not null)
                goto Return;
            deviceProvider = CreateGraphicsDeviceProvider();
            GraphicsDeviceProvider? oldDeviceProvider = InterlockedHelper.CompareExchange(ref _graphicsDeviceProvider, deviceProvider, null);
            if (oldDeviceProvider is null)
            {
                InterlockedHelper.Write(ref _ownedGDP, UnsafeHelper.GetMaxValue<nuint>());
                goto Return;
            }
            deviceProvider.Dispose();
            return oldDeviceProvider;

        Return:
            return deviceProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetGraphicsDeviceProvider([NotNullWhen(true)] out GraphicsDeviceProvider? deviceProvider)
        {
            deviceProvider = InterlockedHelper.Read(ref _graphicsDeviceProvider);
            if (deviceProvider is null)
                return false;
            if (InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                SpinWait.SpinUntil(() => InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) == 0);
            deviceProvider = InterlockedHelper.Read(ref _graphicsDeviceProvider);
            return deviceProvider is not null;
        }

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

        IEnumerable<UIElement?> IElementContainer.GetActiveElements() => GetActiveElements();

        bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

        IRenderer IElementContainer.GetRenderer() => this;

        CoreWindow IElementContainer.GetWindow() => this;

        Vector2 IRenderer.GetPixelsPerPoint() => _pixelsPerPoint;

        Vector2 IRenderer.GetPointsPerPixel() => _pointsPerPixel;

        void IRenderer.Refresh() => Refresh();

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
            UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
            UIElementHelper.ApplyTheme(provider, _overlayElementList);
            UIElementHelper.ApplyTheme(provider, _backgroundElementList);
            if (UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.TitleBackBrush) is D2D1SolidColorBrush backBrush &&
                _windowMaterial == WindowMaterial.Integrated && SystemConstants.VersionLevel >= SystemVersionLevel.Windows_11_21H2)
                FluentHandler.SetTitleBarColor(Handle, (Color)backBrush.Color);
            ConcreteUtils.ResetBlur(this);
        }

        protected virtual void OnMouseDownForElements(ref HandleableMouseEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseDownForElements(elements, ref args);
            UIElementHelper.OnMouseDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnMouseMoveForElements(in MouseEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.MouseMoveData data = default;
            UIElementHelper.OnMouseMoveForElements(elements, args, ref data);
            UIElementHelper.OnMouseMoveForElements(GetBackgroundElements(), args, ref data);
            (SystemCursorType? cursorType, UIElement? hitElement) = data;
            LazyTiny<WeakReference> lastHitElementRefLazy = _lastHitElementRefLazy;
            if (hitElement is null)
            {
                WeakReference? lastHitElementRef = lastHitElementRefLazy.GetValueDirectly();
                if (lastHitElementRef is not null)
                {
                    object? target = lastHitElementRef.Target;
                    if (target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                        handler.OnMouseMove(new MouseEventArgs(lastHitElement.PointToLocal(args.Location), args.Buttons, args.Delta));
                    lastHitElementRef.Target = null;
                }
            }
            else
            {
                WeakReference lastHitElementRef = lastHitElementRefLazy.Value;
                object? target = lastHitElementRef.Target;
                if (!ReferenceEquals(hitElement, target) && target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                    handler.OnMouseMove(new MouseEventArgs(lastHitElement.PointToLocal(args.Location), args.Buttons, args.Delta));
                lastHitElementRef.Target = hitElement;
            }

            Cursor = SystemCursors.GetSystemCursor(cursorType ?? SystemCursorType.Default);
        }

        protected virtual void OnMouseUpForElements(in MouseEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseUpForElements(elements, args);
            UIElementHelper.OnMouseUpForElements(GetBackgroundElements(), args);
        }

        protected virtual void OnMouseScrollForElements(ref HandleableMouseEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnMouseScrollForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnMouseScrollForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyDownForElements(ref KeyEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnKeyDownForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyDownForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnKeyUpForElements(ref KeyEventArgs args)
        {
            IEnumerable<UIElement?> elements = GetOverlayElements();
            if (!elements.HasNonNullItem())
                elements = GetActiveElements();
            UIElementHelper.OnKeyUpForElements(elements, ref args);
            if (args.Handled)
                return;
            UIElementHelper.OnKeyUpForElements(GetBackgroundElements(), ref args);
        }

        protected virtual void OnCharacterInputForElements(ref CharacterEventArgs args)
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

        protected virtual unsafe void RecalculateLayout(Size windowSize, bool callRecalculatePageLayout)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
                _pageRect = Rect.FromXYWH(Point.Empty, ClientSize);
            else
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;

                Vector2 pixelsPerPoint = _pointsPerPixel;
                int activeBorderWidth, drawingOffsetX, drawingOffsetY;
                if (User32.IsZoomed(handle))
                {
                    Rect windowRect;
                    if (!User32.GetWindowRect(handle, &windowRect))
                        Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                    if (!Screen.TryGetScreenInfoFromHwnd(handle, out ScreenInfo screenInfo))
                        screenInfo = default;
                    Rect workingArea = screenInfo.WorkingArea;
                    drawingOffsetX = MathI.Round((workingArea.Left - windowRect.Left) * pixelsPerPoint.X, MidpointRounding.AwayFromZero);
                    drawingOffsetY = MathI.Round((workingArea.Top - windowRect.Top) * pixelsPerPoint.Y, MidpointRounding.AwayFromZero);
                    activeBorderWidth = 0;
                }
                else
                {
                    Vector2 pointsPerPixel = _pixelsPerPoint;
                    activeBorderWidth = _borderWidth;
                    drawingOffsetX = 0;
                    drawingOffsetY = 0;
                }
                _activeBorderWidth = activeBorderWidth;
                _drawingOffsetX = drawingOffsetX;
                _drawingOffsetY = drawingOffsetY;
                int x = windowSize.Width - 1 - drawingOffsetX, y = drawingOffsetY;
                _closeRect = new Rectangle(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                _maxRect = new Rectangle(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                _minRect = new Rectangle(x - UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
                Rect titleBarRect = _titleBarRect = Rect.FromXYWH(drawingOffsetX + 1, drawingOffsetY + 1, Size.Width - 2, 26);
                _pageRect = new Rect(
                    left: drawingOffsetX + activeBorderWidth,
                    top: titleBarRect.Bottom + 1,
                    right: windowSize.Width - drawingOffsetX - activeBorderWidth,
                    bottom: windowSize.Height - activeBorderWidth);
            }
            if (callRecalculatePageLayout && _pageRect.IsValid)
                RecalculatePageLayout(_pageRect);
        }

        protected virtual void RecalculatePageLayout(in Rect pageRect)
        {
            LayoutEngine layoutEngine = RentLayoutEngine();
            layoutEngine.RecalculateLayout(pageRect, GetActiveElements());
            layoutEngine.RecalculateLayout(pageRect, GetOverlayElements());
            ReturnLayoutEngine(layoutEngine);
        }
        #endregion

        #region Rendering
        protected IEnumerable<UIElement> GetOverlayElements() => _overlayElementList;

        protected IEnumerable<UIElement> GetBackgroundElements() => _backgroundElementList;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected D2D1Brush GetBrush(Brush brush)
        {
            if (brush >= Brush._Last)
                throw new ArgumentOutOfRangeException(nameof(brush));
            return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)brush);
        }

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
                Thread.MemoryBarrier();
                RecalculateLayout(GraphicsUtils.ScalingSizeAndConvert(size, _pointsPerPixel), true);
            }
            D2D1DeviceContext? deviceContext = host.BeginDraw();
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
            Rect pageRect = _pageRect;
            if (pageRect.IsValid)
                RenderPage(deviceContext, collector, pageRect, force: true);
            host.EndDraw();

            return host.TryPresent();
        }

        private bool RenderCore_Normal(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, DirtyAreaCollector collector)
        {
            Vector2 pointsPerPixel = _pixelsPerPoint;

            RenderTitle(deviceContext, collector, force: false);
            Rect pageRect = _pageRect;
            if (pageRect.IsValid)
                RenderPage(deviceContext, collector, pageRect, force: false);
            host.EndDraw();

            return collector.TryPresent(pointsPerPixel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPageBackground(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in Rect pageRect)
            => deviceContext.Clear(_windowBaseColor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderOnceContent(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in Rect pageRect) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RenderPage(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in Rect pageRect, bool force)
        {
            using RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, RenderingHelper.RoundInPixel((RectF)pageRect, _pixelsPerPoint), D2D1AntialiasMode.Aliased);
            if (force)
            {
                collector.UsePresentAllModeOnce();
                RenderPageBackground(deviceContext, collector, pageRect);
                RenderOnceContent(deviceContext, collector, pageRect);
            }
            Vector2 pointPerPixel = _pixelsPerPoint;
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
            GraphicsUtils.ClearAndFill(deviceContext, UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.TitleBackBrush), _clearDCColor);
        }

        protected virtual void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force)
        {
            if (_windowMaterial == WindowMaterial.Integrated)
                return;
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            Vector2 pixelsPerPoint = _pixelsPerPoint;

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
                    RectF titleBarRect = RenderingHelper.RoundInPixel(_titleBarRect, pixelsPerPoint);
                    deviceContext.PushAxisAlignedClip(titleBarRect, D2D1AntialiasMode.Aliased);
                    deviceContext.DrawTextLayout(new PointF(_drawingOffsetX + 7.5f, _drawingOffsetY + 1.5f), 
                        titleLayout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleForeBrush));
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
                    RectF minRect = RenderingHelper.RoundInPixel(_minRect, pixelsPerPoint);
                    deviceContext.PushAxisAlignedClip(minRect, D2D1AntialiasMode.Aliased);
                    if (!force)
                        ClearDCForTitle(deviceContext);
                    DebugHelper.ThrowUnless((nuint)Brush.TitleForeDeactiveBrush - 1 == (nuint)Brush.TitleForeBrush);
                    iconStorer.RenderMinimizeButton(deviceContext, (RectangleF)minRect,
                        UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleForeDeactiveBrush - MathHelper.BooleanToNativeUnsigned(TitleBarButtonStatus[0])));
                    deviceContext.PopAxisAlignedClip();
                    collector.MarkAsDirty(minRect);
                }
                if (titleBarStates[2] && (TitleBarButtonChangedStatus[1] || force))
                {
                    RectF maxRect = RenderingHelper.RoundInPixel(_maxRect, pixelsPerPoint);
                    deviceContext.PushAxisAlignedClip(maxRect, D2D1AntialiasMode.Aliased);
                    if (!force)
                    {
                        ClearDCForTitle(deviceContext);
                    }
                    DebugHelper.ThrowUnless((nuint)Brush.TitleForeDeactiveBrush - 1 == (nuint)Brush.TitleForeBrush);
                    D2D1Brush foreBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleForeDeactiveBrush - MathHelper.BooleanToNativeUnsigned(TitleBarButtonStatus[1]));
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
                RectF closeRect = RenderingHelper.RoundInPixel(_closeRect, pixelsPerPoint);
                deviceContext.PushAxisAlignedClip(closeRect, D2D1AntialiasMode.Aliased);
                if (!force)
                {
                    ClearDCForTitle(deviceContext);
                }
                DebugHelper.ThrowUnless((nuint)Brush.TitleForeDeactiveBrush + 1 == (nuint)Brush.TitleCloseButtonActiveBrush);
                iconStorer.RenderCloseButton(deviceContext, (RectangleF)closeRect,
                        UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleForeDeactiveBrush + MathHelper.BooleanToNativeUnsigned(TitleBarButtonStatus[2])));
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
            if (_isSystemPrepareBoosting)
                controller.SetSystemBoosting(true);
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
            WeakReference reference = _focusElementRefLazy.Value;
            if (ReferenceEquals(reference.Target, element))
                return;
            reference.Target = element;
            FocusElementChanged?.Invoke(this, element);
        }

        protected void ClearFocusElement()
        {
            _focusElementRefLazy.GetValueDirectly()?.Target = null;
            FocusElementChanged?.Invoke(this, null);
        }

        public void ClearFocusElement(UIElement elementForValidation)
        {
            WeakReference? reference = _focusElementRefLazy.GetValueDirectly();
            if (reference is null || !ReferenceEquals(reference.Target, elementForValidation))
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
            _controller?.Lock();
            try
            {
                ConcurrentDictionary<Type, UIElement> overlayElementDict = _overlayElementDict;
                UnwrappableList<UIElement> overlayElementList = _overlayElementList;
                UIElement? result;
                if (element is null)
                {
                    if (overlayElementDict.TryRemove(type, out result))
                    {
                        lock (overlayElementList)
                        {
                            overlayElementList.Remove(result);
                            _lastHitElementRefLazy.GetValueDirectly()?.Target = null;
                            WeakReference? recordedRef = _recordedLastHitElementRefLazy.GetValueDirectly();
                            if (recordedRef is not null)
                            {
                                (object? recordedTarget, recordedRef.Target) = (recordedRef.Target, null);
                                if (recordedTarget is UIElement recordedElement && recordedTarget is IMouseMoveHandler handler)
                                    handler.OnMouseMove(new MouseEventArgs(recordedElement.PointToLocal(PointToClient(MouseHelper.GetMousePosition()))));
                            }
                        }
                    }
                    return result;
                }
                WeakReference? lastHitRef = _lastHitElementRefLazy.GetValueDirectly();
                if (lastHitRef is not null)
                {
                    (object? recordedTarget, lastHitRef.Target) = (lastHitRef.Target, null);
                    if (recordedTarget is not null)
                        _recordedLastHitElementRefLazy.Value.Target = recordedTarget;
                }
                if (!overlayElementDict.TryGetValue(type, out result))
                {
                    lock (overlayElementList)
                    {
                        IThemeResourceProvider? resourceProvider = _resourceProvider;
                        if (resourceProvider is not null)
                            element.ApplyTheme(resourceProvider);

                        LayoutEngine layoutEngine = RentLayoutEngine();
                        layoutEngine.RecalculateLayout(_pageRect, element);
                        ReturnLayoutEngine(layoutEngine);

                        overlayElementDict.TryAdd(type, element);
                        overlayElementList.Add(element);
                    }
                    return null;
                }
                if (result is null || predicate is null || predicate.Invoke(result))
                {
                    lock (overlayElementList)
                    {
                        IThemeResourceProvider? resourceProvider = _resourceProvider;
                        if (resourceProvider is not null)
                            element.ApplyTheme(resourceProvider);

                        LayoutEngine layoutEngine = RentLayoutEngine();
                        layoutEngine.RecalculateLayout(_pageRect, element);
                        ReturnLayoutEngine(layoutEngine);

                        int index = result is null ? -1 : overlayElementList.IndexOf(result);
                        overlayElementDict[type] = element;
                        if (index > -1)
                            overlayElementList[index] = element;
                        else
                            overlayElementList.Add(element);

                        return result;
                    }
                }
                return null;
            }
            finally
            {
                _controller?.Unlock();
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenContextMenu(UIElement elementRelativeTo, ContextMenu.ContextMenuItem[] items, Point location)
        {
            if (!items.HasAnyItem())
                return;

            OpenContextMenuCore(items, elementRelativeTo.PointToGlobal(location));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenContextMenu(ContextMenu.ContextMenuItem[] items, Point location)
        {
            if (!items.HasAnyItem())
                return;

            OpenContextMenuCore(items, location);
        }

        private void OpenContextMenuCore(ContextMenu.ContextMenuItem[] items, Point location)
        {
            ContextMenu contextMenu = new ContextMenu(this, items);
            ChangeOverlayElement(contextMenu)?.Dispose();
            Rect pageRect = _pageRect;
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
            => (ChangeOverlayElement(elementType, null, _element => ReferenceEquals(_element, elementForValidate)) as IDisposable)?.Dispose();

        protected unsafe void ApplyTheme(IThemeResourceProvider provider)
        {
            RenderingController? controller = _controller;
            if (controller is not null)
            {
                controller.Lock();
                controller.WaitForRendering();
            }
            SimpleGraphicsHost? host = _host;
            if (host is null || host.IsDisposed)
                return;
            DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider, provider);
            ApplyThemeCore(provider);
            if (TryGetWindowListSnapshot(_childrenReferenceList, out NativeMemoryPool? pool,
                out TypedNativeMemoryBlock<GCHandle> handles, out int count))
            {
                try
                {
                    DebugHelper.ThrowIf(count <= 0);
                    GCHandle* ptr = handles.NativePointer;
                    for (int i = 0; i < count; i++)
                    {
                        GCHandle handle = ptr[i];
                        if (!handle.IsAllocated || handle.Target is not CoreWindow window || window.IsDisposed)
                            continue;
                        window.ApplyTheme(provider.Clone());
                    }
                }
                finally
                {
                    pool.Return(handles);
                }
            }
            if (controller is not null)
            {
                TriggerResizeCore(controller, _sizeModeState);
                controller.Unlock();
            }
        }
        #endregion

        #region Static Methods
        internal static unsafe void NotifyThemeChanged(IThemeContext themeContext)
        {
            if (!TryGetWindowListSnapshot(_rootWindowList, out NativeMemoryPool? pool,
                    out TypedNativeMemoryBlock<GCHandle> handles, out int count))
                return;
            try
            {
                DebugHelper.ThrowIf(count <= 0);
                GCHandle* ptr = handles.NativePointer;
                for (int i = 0; i < count; i++)
                {
                    GCHandle handle = ptr[i];
                    if (!handle.IsAllocated || handle.Target is not CoreWindow window || window.IsDisposed)
                        continue;
                    D2D1DeviceContext? deviceContext = window._deviceContext;
                    if (deviceContext is null || deviceContext.IsDisposed)
                        continue;
                    window.ApplyTheme(ThemeResourceProvider.CreateResourceProvider(window, themeContext));
                }
            }
            finally
            {
                pool.Return(handles);
            }
        }

        private static unsafe bool TryGetWindowListSnapshot(UnwrappableList<GCHandle> windowList,
            [NotNullWhen(true)] out NativeMemoryPool? pool, [NotNullWhen(true)] out TypedNativeMemoryBlock<GCHandle> handles, out int count)
        {
            lock (windowList)
            {
                count = windowList.Count;
                if (count <= 0)
                    goto Failed;
                pool = NativeMemoryPool.Shared;
                count -= ClearInvalidHandles(pool, windowList, count);
                if (count <= 0)
                    goto Failed;
                handles = pool.Rent<GCHandle>(count);
                fixed (GCHandle* source = windowList.Unwrap())
                {
                    GCHandle* destination = handles.NativePointer;
                    UnsafeHelper.CopyBlockUnaligned(destination, source, (uint)(count * sizeof(GCHandle)));
                }
                return true;
            }

        Failed:
            pool = null;
            handles = TypedNativeMemoryBlock<GCHandle>.Empty;
            return false;

            static int ClearInvalidHandles(NativeMemoryPool pool, UnwrappableList<GCHandle> list, int count)
            {
                TypedNativeMemoryBlock<int> removeIndicesBuffer = pool.Rent<int>(count);
                try
                {
                    int* removeIndicesPtr = removeIndicesBuffer.NativePointer;
                    int removeIndicesCount = 0;
                    {
                        ref GCHandle handleRef = ref UnsafeHelper.GetArrayDataReference(list.Unwrap());
                        for (int i = 0; i < count; i++)
                        {
                            GCHandle handle = UnsafeHelper.AddTypedOffset(ref handleRef, i);
                            if (!handle.IsAllocated || handle.Target is not CoreWindow window || window.IsDisposed)
                            {
                                handle.Free();
                                removeIndicesPtr[removeIndicesCount++] = i;
                            }
                        }
                    }
                    for (int j = removeIndicesCount - 1; j >= 0; j--)
                    {
                        // 從最後面開始減，提高效能
                        list.RemoveAt(removeIndicesPtr[j]);
                    }
                    DebugHelper.ThrowIf(removeIndicesCount > count);
                    return removeIndicesCount;
                }
                finally
                {
                    pool.Return(removeIndicesBuffer);
                }
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
                DisposeHelper.SwapDisposeInterlocked(ref _host);
                DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
                DisposeElements(GetElements());

                if (InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                    SpinWait.SpinUntil(() => InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0);
                if (InterlockedHelper.Read(ref _ownedGDP) != 0)
                    DisposeHelper.SwapDisposeInterlocked(ref _graphicsDeviceProvider);
                else
                    InterlockedHelper.Write(ref _graphicsDeviceProvider, null);
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
            ref UIElement? elementRef = ref UnsafeHelper.GetArrayDataReference(elements);
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
