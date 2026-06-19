using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
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
using ConcreteUI.Layout;
using ConcreteUI.Layout.Internals;
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

namespace ConcreteUI.Windows;

public abstract partial class CoreWindow : IRenderer, IElementContainer, ICoordinateTranslator
{
    [StructLayout(LayoutKind.Auto)]
    protected ref struct WindowRenderingData
    {
        public Rectangle MinimizeButtonBounds, MaximizeButtonBounds, CloseButtonBounds, PageBounds, TitleBarBounds;
        public Point DrawingOffset;
        public int ActiveBorderWidth;
    }

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

    #endregion

    #region Fields
    private readonly LazyTiny<WeakReference> _focusElementRefLazy, _lastHitElementRefLazy, _recordedLastHitElementRefLazy;
    private readonly object _syncLock;
    private readonly WindowMaterial _windowMaterial;

    private SimpleGraphicsHost? _host;
    private DirtyAreaCollector? _collector;
    private RenderingController? _controller;
    private UIElement? _overlayElement;
    private IThemeResourceProvider? _resourceProvider;
    private WindowMaterial _actualWindowMaterial;
    private long _updateFlags = Booleans.TrueLong;
    #endregion

    #region Rendering Fields
    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    private GraphicsDeviceProvider? _graphicsDeviceProvider;
    private D2D1DeviceContext? _deviceContext;
    private DWriteTextLayout? _titleLayout;
    private D2D1ColorF _clearDCColor, _windowBaseColor;
    private Point _drawingOffset;
    private ulong
        _minimizeButtonLocation, _minimizeButtonSize,
        _maximizeButtonLocation, _maximizeButtonSize,
        _closeButtonLocation, _closeButtonSize,
        _pageLocation, _pageSize,
        _titleBarLocation, _titleBarSize;
    private nuint _ownedGDP, _recreateGraphicsDeviceProviderBarrier, _recalculateLayoutVersion;
    private int _activeBorderWidth;

    protected BitVector64 _titleBarButtonStatus, _titleBarButtonChangedStatus;
    #endregion

    #region Static Properties
    public static LayoutNode PageWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PageWidthNode.Instance;
    }

    public static LayoutNode PageHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PageHeightNode.Instance;
    }
    #endregion

    #region Properties
    public UIElement? FocusedElement
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _focusElementRefLazy.GetValueDirectly()?.Target as UIElement;
    }

    public WindowMaterial WindowMaterial => _windowMaterial;

    public WindowMaterial ActualWindowMaterial => _actualWindowMaterial;

    public D2D1ColorF ClearDCColor => _clearDCColor;

    public D2D1ColorF WindowBaseColor => _windowBaseColor;

    public Rectangle MinimizeButtonBounds
    {
        get
        {
            ulong location, size;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                location = Volatile.Read(ref _minimizeButtonLocation);
                size = Volatile.Read(ref _minimizeButtonSize);
            }
            while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(location, size);
        }
    }

    public Point MinimizeButtonLocation
    {
        get
        {
            ref readonly ulong resultRef = ref _minimizeButtonLocation;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    public Size MinimizeButtonSize
    {
        get
        {
            ref readonly ulong resultRef = ref _minimizeButtonSize;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
    }

    public Rectangle MaximizeButtonBounds
    {
        get
        {
            ulong location, size;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                location = Volatile.Read(ref _maximizeButtonLocation);
                size = Volatile.Read(ref _maximizeButtonSize);
            }
            while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(location, size);
        }
    }

    public Point MaximizeButtonLocation
    {
        get
        {
            ref readonly ulong resultRef = ref _maximizeButtonLocation;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    public Size MaximizeButtonSize
    {
        get
        {
            ref readonly ulong resultRef = ref _maximizeButtonSize;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
    }

    public Rectangle CloseButtonBounds
    {
        get
        {
            ulong location, size;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                location = Volatile.Read(ref _closeButtonLocation);
                size = Volatile.Read(ref _closeButtonSize);
            }
            while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(location, size);
        }
    }

    public Point CloseButtonLocation
    {
        get
        {
            ref readonly ulong resultRef = ref _closeButtonLocation;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    public Size CloseButtonSize
    {
        get
        {
            ref readonly ulong resultRef = ref _closeButtonSize;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
    }

    public Rectangle TitleBarBounds
    {
        get
        {
            ulong location, size;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                location = Volatile.Read(ref _titleBarLocation);
                size = Volatile.Read(ref _titleBarSize);
            }
            while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(location, size);
        }
    }

    public Point TitleBarLocation
    {
        get
        {
            ref readonly ulong resultRef = ref _titleBarLocation;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    public Size TitleBarSize
    {
        get
        {
            ref readonly ulong resultRef = ref _titleBarSize;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
    }

    public Rectangle PageBounds
    {
        get
        {
            ulong location, size;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                location = Volatile.Read(ref _pageLocation);
                size = Volatile.Read(ref _pageSize);
            }
            while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(location, size);
        }
    }

    public Point PageLocation
    {
        get
        {
            ref readonly ulong resultRef = ref _pageLocation;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    public Size PageSize
    {
        get
        {
            ref readonly ulong resultRef = ref _pageSize;
            ref readonly nuint versionRef = ref _recalculateLayoutVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
    }
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
        InitializeElements();
        if (parent is null)
            ApplyTheme(ThemeResourceProvider.CreateResourceProviderUnsafe(deviceContext, ThemeManager.CurrentTheme, _actualWindowMaterial));
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
        Monitor.Enter(_syncLock);
        try
        {
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
        catch (Exception)
        {
            Monitor.Exit(_syncLock);
        }
    }

    private unsafe void RecreateResourcesFromGDREvent(GraphicsDeviceProvider? deviceProvider, IThemeResourceProvider? resourceProvider)
    {
        try
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
            UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
            controller.Unlock();
        }
        finally
        {
            Monitor.Exit(_syncLock);
        }
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

    void IRenderingControl.Render(RenderingController controller)
    {
        RenderingFlags flags = controller.GetAndResetRenderingFlags();
        if (!RenderCore(controller, flags))
            controller.RequestUpdateUnsafe(flags);
    }

    public IThemeResourceProvider? GetThemeResourceProvider() => InterlockedHelper.Read(ref _resourceProvider);

    IEnumerable<UIElement?> IElementContainer.GetActiveElements() => GetActiveElements();

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    IRenderer IElementContainer.GetRenderer() => this;

    CoreWindow IElementContainer.GetWindow() => this;

    Point ICoordinateTranslator.PageToWindow(UIElement element, Point point) => PageToWindow(element, point);

    PointF ICoordinateTranslator.PageToWindow(UIElement element, PointF point) => PageToWindow(element, point);

    Point ICoordinateTranslator.WindowToPage(UIElement element, Point point) => WindowToPage(element, point);

    PointF ICoordinateTranslator.WindowToPage(UIElement element, PointF point) => WindowToPage(element, point);

    Vector2 IRenderer.GetPixelsPerPoint() => _pixelsPerPoint;

    Vector2 IRenderer.GetPointsPerPixel() => _pointsPerPixel;

    void IRenderer.Refresh() => Refresh();

    void IRenderer.Update() => Update();

    private bool IsBackgroundOpaque() => _actualWindowMaterial == WindowMaterial.None;
    #endregion

    #region Abstract Methods
    protected abstract void InitializeElements();

    protected abstract IEnumerable<UIElement?> GetActiveElements();
    #endregion

    #region Virtual Methods
    public virtual IEnumerable<UIElement?> GetElements() => GetActiveElements()
        .ConcatOptimized(GetOverlayElement());

    protected virtual void ApplyThemeCore(IThemeResourceProvider provider)
    {
        _clearDCColor = provider.TryGetColor(ThemeConstants.ClearDCColorNode, out D2D1ColorF color) ? color : default;
        _windowBaseColor = provider.TryGetColor(ThemeConstants.WindowBaseColorNode, out color) ? color : default;
        GetOverlayElement()?.ApplyTheme(provider);
        UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
        ConcreteUtils.ResetBlur(this);
    }

    protected virtual Point PageToWindow(UIElement element, Point point) => PageToWindow(point);

    protected virtual PointF PageToWindow(UIElement element, PointF point) => PageToWindow(point);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Point PageToWindow(Point point)
    {
        Point baseLoc = PageLocation;
        return new Point(baseLoc.X + point.X, baseLoc.Y + point.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected PointF PageToWindow(PointF point)
    {
        Point baseLoc = PageLocation;
        return new PointF(baseLoc.X + point.X, baseLoc.Y + point.Y);
    }

    protected virtual Point WindowToPage(UIElement element, Point point) => WindowToPage(point);

    protected virtual PointF WindowToPage(UIElement element, PointF point) => WindowToPage(point);

    protected Point WindowToPage(Point point)
    {
        Point baseLoc = PageLocation;
        return new Point(
            x: point.X - baseLoc.X,
            y: point.Y - baseLoc.Y
            );
    }

    protected PointF WindowToPage(PointF point)
    {
        PointF baseLoc = PageLocation;
        return new PointF(
            x: point.X - baseLoc.X,
            y: point.Y - baseLoc.Y
            );
    }

    protected virtual Point PointToPixel(Point point) => GraphicsUtils.ScalingPoint(point, _pixelsPerPoint);

    protected virtual PointF PointToPixel(PointF point) => GraphicsUtils.ScalingPoint(point, _pixelsPerPoint);

    protected virtual Point PixelToPoint(Point point) => GraphicsUtils.ScalingPoint(point, _pointsPerPixel);

    protected virtual PointF PixelToPoint(PointF point) => GraphicsUtils.ScalingPoint(point, _pointsPerPixel);

    protected virtual void OnMouseDownForElements(ref HandleableMouseEventArgs args)
    {
        if (args.Handled)
            return;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnMouseDownForElement(overlayElement, ref args);
        else
            UIElementHelper.OnMouseDownForElements(GetActiveElements(), ref args);
    }

    protected virtual void OnMouseMoveForElements(in MouseEventArgs args)
    {
        UIElementHelper.MouseMoveData data = default;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnMouseMoveForElement(overlayElement, args, ref data);
        else
            UIElementHelper.OnMouseMoveForElements(GetActiveElements(), args, ref data);
        Cursor = SystemCursors.GetSystemCursor(data.CursorType ?? SystemCursorType.Default);
        ChangeLastHitElement(data.LastHitElement, args);
    }

    protected virtual void OnMouseUpForElements(in MouseEventArgs args)
    {
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnMouseUpForElement(overlayElement, args);
        else
            UIElementHelper.OnMouseUpForElements(GetActiveElements(), args);
    }

    protected virtual void OnMouseScrollForElements(ref HandleableMouseEventArgs args)
    {
        if (args.Handled)
            return;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnMouseScrollForElement(overlayElement, ref args);
        else
            UIElementHelper.OnMouseScrollForElements(GetActiveElements(), ref args);
    }

    protected virtual void OnKeyDownForElements(ref KeyEventArgs args)
    {
        if (args.Handled)
            return;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnKeyDownForElement(overlayElement, ref args);
        else
            UIElementHelper.OnKeyDownForElements(GetActiveElements(), ref args);
    }

    protected virtual void OnKeyUpForElements(ref KeyEventArgs args)
    {
        if (args.Handled)
            return;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnKeyUpForElement(overlayElement, ref args);
        else
            UIElementHelper.OnKeyUpForElements(GetActiveElements(), ref args);
    }

    protected virtual void OnCharacterInputForElements(ref CharacterEventArgs args)
    {
        if (args.Handled)
            return;
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnCharacterInputForElement(overlayElement, ref args);
        else
            UIElementHelper.OnCharacterInputForElements(GetActiveElements(), ref args);
    }

    protected virtual void OnDpiChangedForElements(in DpiChangedEventArgs args)
    {
        UIElementHelper.OnDpiChangedForElement(GetOverlayElement(), in args);
        UIElementHelper.OnDpiChangedForElements(GetActiveElements(), in args);
    }

    private void ChangeLastHitElement(UIElement? element, in MouseEventArgs args)
    {
        LazyTiny<WeakReference> lastHitElementRefLazy = _lastHitElementRefLazy;
        if (element is null)
        {
            WeakReference? lastHitElementRef = lastHitElementRefLazy.GetValueDirectly();
            if (lastHitElementRef is not null)
            {
                object? target = lastHitElementRef.Target;
                if (target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                    handler.OnMouseMove(new MouseEventArgs(lastHitElement.PageToLocal(args.Location), args.Buttons, args.Delta));
                lastHitElementRef.Target = null;
            }
        }
        else
        {
            WeakReference lastHitElementRef = lastHitElementRefLazy.Value;
            object? target = lastHitElementRef.Target;
            if (!ReferenceEquals(element, target) && target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                handler.OnMouseMove(new MouseEventArgs(lastHitElement.PageToLocal(args.Location), args.Buttons, args.Delta));
            lastHitElementRef.Target = element;
        }
    }

    protected unsafe virtual void RecalculateLayout(ref WindowRenderingData data, Size windowSize)
    {
        Rectangle pageBounds;
        Size pageSize;
        if (_isIntegratedMaterial)
        {
            pageSize = ClientSize;
            pageBounds = new Rectangle(Point.Empty, pageSize);
        }
        else
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return;

            Vector2 pointsPerPixel = _pointsPerPixel;
            int activeBorderWidth, drawingOffsetX, drawingOffsetY;
            if (User32.IsZoomed(handle))
            {
                Rect windowRect;
                if (!User32.GetWindowRect(handle, &windowRect))
                    Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                if (!Screen.TryGetScreenInfoFromHwnd(handle, out ScreenInfo screenInfo))
                    screenInfo = default;
                Rect workingArea = screenInfo.WorkingArea;
                drawingOffsetX = MathI.Round((workingArea.Left - windowRect.Left) * pointsPerPixel.X, MidpointRounding.AwayFromZero);
                drawingOffsetY = MathI.Round((workingArea.Top - windowRect.Top) * pointsPerPixel.Y, MidpointRounding.AwayFromZero);
                activeBorderWidth = 0;
            }
            else
            {
                activeBorderWidth = _borderWidth;
                drawingOffsetX = 0;
                drawingOffsetY = 0;
            }
            data.ActiveBorderWidth = activeBorderWidth;
            data.DrawingOffset = new Point(drawingOffsetX, drawingOffsetY);
            int x = windowSize.Width - 1 - drawingOffsetX, y = drawingOffsetY;
            data.CloseButtonBounds = new Rectangle(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
            data.MaximizeButtonBounds = new Rectangle(x -= UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
            data.MinimizeButtonBounds = new Rectangle(x - UIConstantsPrivate.TitleBarButtonSizeWidth, y, UIConstantsPrivate.TitleBarButtonSizeWidth, UIConstantsPrivate.TitleBarButtonSizeHeight);
            Rectangle titleBarBounds = new Rectangle(drawingOffsetX + 1, drawingOffsetY + 1, Size.Width - 2, 26);
            pageBounds = Rectangle.FromLTRB(
                left: drawingOffsetX + activeBorderWidth,
                top: titleBarBounds.Bottom + 1,
                right: windowSize.Width - drawingOffsetX - activeBorderWidth,
                bottom: windowSize.Height - activeBorderWidth);
            pageSize = pageBounds.Size;
            data.TitleBarBounds = titleBarBounds;
        }

        data.PageBounds = pageBounds;
    }

    protected virtual void RecalculatePageLayout(Size pageSize)
    {
        using LayoutEngineRentScope engine = LayoutEngine.Rent();
        engine.RecalculateLayout(pageSize, GetActiveElements());
        engine.RecalculateLayout(pageSize, GetOverlayElement());
        Thread.MemoryBarrier();
    }
    #endregion

    #region Rendering
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected UIElement? GetOverlayElement() => _overlayElement;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected D2D1Brush GetBrush(Brush brush)
    {
        if (brush >= Brush._Last)
            throw new ArgumentOutOfRangeException(nameof(brush));
        return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)brush);
    }

    [Inline]
    private bool RenderCore(RenderingController controller, RenderingFlags flags)
    {
        SimpleGraphicsHost? host = _host;
        if (host is null || host.IsDisposed)
            return false;
        DirtyAreaCollector? collector = _collector;
        if (collector is null)
            return false;

        WindowRenderingData data = new WindowRenderingData()
        {
            MinimizeButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_minimizeButtonLocation, _minimizeButtonSize),
            MaximizeButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_maximizeButtonLocation, _maximizeButtonSize),
            CloseButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_closeButtonLocation, _closeButtonSize),
            PageBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_pageLocation, _pageSize),
            TitleBarBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_titleBarLocation, _titleBarSize),
            DrawingOffset = _drawingOffset,
            ActiveBorderWidth = _activeBorderWidth
        };
        bool redrawAll = flags.HasFlagFast(RenderingFlags.RedrawAll);
        if (flags.HasFlagFast(RenderingFlags.Resize))
        {
            bool resizeTemporarily = flags.HasFlagFast(RenderingFlags._ResizeTemporarilyFlag);
            Size size = RawClientSize;
            if (size.Width <= 0 || size.Height <= 0)
                return false;
            if (resizeTemporarily)
                redrawAll |= host.ResizeTemporarily(size);
            else
                redrawAll |= host.Resize(size);
            Thread.MemoryBarrier();
            if (redrawAll)
            {
                RecalculateLayout(
                    data: ref data,
                    windowSize: GraphicsUtils.ScalingSizeAndConvert(size, _pointsPerPixel));
                BoundsHelper.SaveBoundsToUInt64Fields(data.MinimizeButtonBounds, ref _minimizeButtonLocation, ref _minimizeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(data.MaximizeButtonBounds, ref _maximizeButtonLocation, ref _maximizeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(data.CloseButtonBounds, ref _closeButtonLocation, ref _closeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(data.PageBounds, ref _pageLocation, ref _pageSize);
                BoundsHelper.SaveBoundsToUInt64Fields(data.TitleBarBounds, ref _titleBarLocation, ref _titleBarSize);
                _drawingOffset = data.DrawingOffset;
                _activeBorderWidth = data.ActiveBorderWidth;
                InterlockedHelper.Increment(ref _recalculateLayoutVersion);

                Size pageSize = data.PageBounds.Size;
                if (pageSize.IsValid())
                    RecalculatePageLayout(pageSize);
            }
            flags = controller.GetAndResetRenderingFlags();
            redrawAll |= flags.HasFlagFast(RenderingFlags.RedrawAll);
            if (resizeTemporarily || flags.HasFlagFast(RenderingFlags.Resize))
                controller.RequestUpdateAndResize(flags.HasFlagFast(RenderingFlags._ResizeTemporarilyFlag), redrawAll: false);
        }
        D2D1DeviceContext? deviceContext = host.BeginDraw();
        if (deviceContext is null || deviceContext.IsDisposed)
            return true;

        ClearTypeSwitcher.SetClearType(deviceContext, false);

        if (redrawAll)
            return RenderCore_RedrawAll(host, deviceContext, in data);
        else
            return RenderCore_Normal(host, deviceContext, collector, in data);
    }

    private bool RenderCore_RedrawAll(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, in WindowRenderingData data)
    {
        DirtyAreaCollector collector = DirtyAreaCollector.Empty;
        RenderTitle(deviceContext, collector, force: true, in data);
        Rectangle pageBounds = data.PageBounds;
        if (pageBounds.IsValid())
        {
            using RegionalRenderingContext context = RegionalRenderingContext.Create(deviceContext, collector, _pixelsPerPoint,
                pageBounds, D2D1AntialiasMode.Aliased, IsBackgroundOpaque(), out _);
            RenderPage(context, in data);
        }
        host.EndDraw();

        return host.TryPresent();
    }

    private bool RenderCore_Normal(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in WindowRenderingData data)
    {
        Vector2 pixelsPerPoint = _pixelsPerPoint;

        RenderTitle(deviceContext, collector, force: false, in data);
        Rectangle pageBounds = data.PageBounds;
        if (pageBounds.IsValid())
        {
            using RegionalRenderingContext context = RegionalRenderingContext.Create(deviceContext, collector, pixelsPerPoint,
                pageBounds, D2D1AntialiasMode.Aliased, IsBackgroundOpaque(), out _);
            RenderPage(context, in data);
        }
        host.EndDraw();

        return collector.TryPresent(pixelsPerPoint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void RenderPageBackground(in RegionalRenderingContext context, in WindowRenderingData data)
        => context.Clear(_windowBaseColor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
    {
        bool force = context.IsForceRendering;
        if (force)
            RenderPageBackground(context, in data);
        UIElementHelper.RenderElements(context, GetActiveElements(), ignoreNeedRefresh: force);
        UIElementHelper.RenderElement(context, _overlayElement, ignoreNeedRefresh: force || context.HasAnyDirtyArea());
    }

    protected virtual void ClearDCForTitle(D2D1DeviceContext deviceContext)
    {
        if (_isIntegratedMaterial)
        {
            deviceContext.Clear();
            return;
        }
        GraphicsUtils.ClearAndFill(deviceContext, UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.TitleBackBrush), _clearDCColor);
    }

    protected virtual void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force, in WindowRenderingData data)
    {
        if (_isIntegratedMaterial)
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
                Point drawingOffset = _drawingOffset;
                RectF titleBarRect = RenderingHelper.RoundInPixel(data.TitleBarBounds, pixelsPerPoint);
                deviceContext.PushAxisAlignedClip(titleBarRect, D2D1AntialiasMode.Aliased);
                deviceContext.DrawTextLayout(new PointF(drawingOffset.X + 7.5f, drawingOffset.Y + 1.5f),
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
                RectF minRect = RenderingHelper.RoundInPixel(data.MinimizeButtonBounds, pixelsPerPoint);
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
                RectF maxRect = RenderingHelper.RoundInPixel(data.MaximizeButtonBounds, pixelsPerPoint);
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
            RectF closeRect = RenderingHelper.RoundInPixel(data.CloseButtonBounds, pixelsPerPoint);
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
    private void SystemEvents_DisplaySettingsChanging(object? sender, EventArgs e)
        => _controller?.Lock();

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        RenderingController? controller = _controller;
        if (controller is null)
            return;
        UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
        controller.Unlock();
    }

    private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
                _controller?.Lock();
                break;
            case SessionSwitchReason.SessionUnlock:
                {
                    RenderingController? controller = _controller;
                    if (controller is not null)
                    {
                        UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
                        controller.Unlock();
                    }
                }
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
                {
                    RenderingController? controller = _controller;
                    if (controller is not null)
                    {
                        UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
                        controller.Unlock();
                    }
                }
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
        UpdateAndResize();
    }

    private void UpdateFirstTime()
    {
        RenderingController controller = new RenderingController(this, GetWindowFps(Handle));
        if (_isSystemPrepareBoosting)
            controller.SetSystemBoosting(true);
        _controller = controller;
        UpdateCoreUnchecked(controller);
    }

    [Inline(InlineBehavior.Remove)]
    private static void RefreshCoreUnchecked(RenderingController controller) => controller.RequestUpdate(false);

    [Inline(InlineBehavior.Remove)]
    private static void RefreshCore(RenderingController? controller)
    {
        if (controller is null)
            return;
        RefreshCoreUnchecked(controller);
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
    private static void UpdateAndResizeCoreUnchecked(RenderingController controller, ref bool sizeModeState)
        => controller.RequestUpdateAndResize(Volatile.Read(ref sizeModeState));

    [Inline(InlineBehavior.Remove)]
    private static void UpdateAndResizeCore(RenderingController? controller, ref bool sizeModeState)
    {
        if (controller is null)
            return;
        UpdateAndResizeCoreUnchecked(controller, ref sizeModeState);
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
        try
        {
            lock (_syncLock)
            {
                host.GetDeviceContext().Dpi = new PointF(dpi.X, dpi.Y);
                OnDpiChangedForElements(new DpiChangedEventArgs(dpi, pointsPerPixel, pixelsPerPoint));
            }
        }
        finally
        {
            UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
            controller.Unlock();
        }
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
    #endregion

    #region Normal Methods
    protected void UpdateAndResize() => UpdateAndResizeCore(_controller, ref _sizeModeState);

    protected void Update() => UpdateCore(_controller);

    protected void Refresh() => RefreshCore(_controller);

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

    protected UIElement? ChangeOverlayElement(UIElement? element)
    {
        RenderingController? controller = _controller;
        if (controller is not null)
        {
            controller.Lock();
            controller.WaitForRendering();
        }
        try
        {
            UIElement? oldElement;
            lock (_syncLock)
            {
                if (element is not null)
                {
                    IThemeResourceProvider? provider = _resourceProvider;
                    if (provider is not null)
                        element.ApplyTheme(provider);
                }
                oldElement = ReferenceHelper.Exchange(ref _overlayElement, element);
            }

            OnOverlayLayerChanged(element, oldElement);
            return oldElement;
        }
        finally
        {
            if (controller is not null)
            {
                UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
                controller.Unlock();
            }
        }
    }

    protected void ChangeOverlayElement(UIElement? element, UIElement? oldElement)
    {
        RenderingController? controller = _controller;
        if (controller is not null)
        {
            controller.Lock();
            controller.WaitForRendering();
        }
        try
        {
            lock (_syncLock)
            {
                if (!ReferenceEquals(_overlayElement, oldElement))
                    return;
                if (element is not null)
                {
                    IThemeResourceProvider? provider = _resourceProvider;
                    if (provider is not null)
                        element.ApplyTheme(provider);
                }
                _overlayElement = element;
            }

            OnOverlayLayerChanged(element, oldElement);
        }
        finally
        {
            if (controller is not null)
            {
                UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
                controller.Unlock();
            }
        }
    }

    private void OnOverlayLayerChanged(UIElement? element, UIElement? oldElement)
    {
        if (element is null)
        {
            WeakReference? recordedLastHitElementRef = _recordedLastHitElementRefLazy.GetValueDirectly();
            if (recordedLastHitElementRef is not null)
            {
                _lastHitElementRefLazy.Value.Target = recordedLastHitElementRef.Target;
                recordedLastHitElementRef.Target = null;
            }
        }
        else if (oldElement is null)
        {
            WeakReference? lastHitElementRef = _lastHitElementRefLazy.GetValueDirectly();
            if (lastHitElementRef is not null)
            {
                object? target = lastHitElementRef.Target;
                if (target is not null)
                    _recordedLastHitElementRefLazy.Value.Target = target;
            }
        }
        OnMouseMoveForElements(new MouseEventArgs(WindowToPage(PointToClient(MouseHelper.GetMousePosition()))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OpenContextMenu(UIElement elementRelativeTo, ContextMenu.ContextMenuItem[] items, Point location)
    {
        if (!items.HasAnyItem())
            return;

        OpenContextMenuCore(items, elementRelativeTo.LocalToPage(location));
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
        Rectangle pageBounds = PageBounds;
        if (location.X + contextMenu.Width >= pageBounds.Right)
            location.X = location.X - contextMenu.Width + 1;
        if (location.Y + contextMenu.Height >= pageBounds.Bottom)
            location.Y = location.Y - contextMenu.Height + 1;
        contextMenu.Location = location;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CloseOverlayElement(UIElement elementForValidate)
        => ChangeOverlayElement(null, elementForValidate);

    protected unsafe void ApplyTheme(IThemeResourceProvider provider)
    {
        RenderingController? controller = _controller;
        if (controller is not null)
        {
            controller.Lock();
            controller.WaitForRendering();
        }
        try
        {
            lock (_syncLock)
            {
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
            }
        }
        finally
        {
            if (controller is not null)
            {
                UpdateAndResizeCoreUnchecked(controller, ref _sizeModeState);
                controller.Unlock();
            }
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
            UIElementHelper.DisposeForElements(GetElements());
            UIElementHelper.DisposeForElement(_overlayElement);

            if (InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                SpinWait.SpinUntil(() => InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0);
            if (InterlockedHelper.Read(ref _ownedGDP) != 0)
                DisposeHelper.SwapDisposeInterlocked(ref _graphicsDeviceProvider);
            else
                InterlockedHelper.Write(ref _graphicsDeviceProvider, null);
        }
        _overlayElement = null;
        SequenceHelper.Clear(_brushes);
        base.DisposeCore(disposing);
    }
    #endregion
}
