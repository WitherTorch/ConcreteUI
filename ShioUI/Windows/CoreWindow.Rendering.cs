using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using InlineMethod;

using Microsoft.Win32;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;
using RiceTea.Core.Threading;

using ShioUI.Controls;
using ShioUI.Extensions;
using ShioUI.Graphics;
using ShioUI.Graphics.Extensions;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Hosts;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Graphics.Native.DXGI;
using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Layout;
using ShioUI.Layout.Internals;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class CoreWindow : IRenderable, IRenderWindow
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

    #endregion

    #region Fields
    private readonly GCHandle[] _recordedMouseDownHitElementRefs = new GCHandle[7]; // MouseButtons._Mask == 0x7F, The count of available bit fields always be 7;
    private readonly Lock _syncLock = new Lock();
    private readonly WindowMaterial _windowMaterial;

    private SimpleGraphicsHost? _host;
    private DirtyAreaCollector? _collector;
    private RenderingController? _controller;
    private UIElement? _overlayElement;
    private IThemeResourceProvider? _resourceProvider;
    private WindowMaterial _actualWindowMaterial;
    private GCHandle _lastMouseMoveHitElementRef, _recordedLastMouseMoveHitElementRef, _focusElementRef;
    private long _updateFlags = Booleans.TrueLong;
    #endregion

    #region Rendering Fields
    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    private GraphicsDeviceProvider? _graphicsDeviceProvider;
    private D2D1DeviceContext? _deviceContext;
    private DWriteTextLayout? _titleLayout;
    private D2D1ColorF _clearDCColor, _windowBaseColor;
    private Point _drawingOffset;
    private ulong _resizeTimestamp, _renderTimestamp,
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
        get => PageWidthLayoutNode.Instance;
    }

    public static LayoutNode PageHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PageHeightLayoutNode.Instance;
    }
    #endregion

    #region Properties
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

    #region Init
    private static GraphicsDeviceProvider CreateGraphicsDeviceProvider()
    {
        string targetGpuName = ShioSettings.TargetGpuName;
        bool isDebug = ShioSettings.UseDebugMode;
        if (StringHelper.IsNullOrEmpty(targetGpuName))
            return new GraphicsDeviceProvider(DXGIGpuPreference.Invalid, isDebug);
        if (targetGpuName.StartsWith('#'))
        {
            DXGIGpuPreference preference = targetGpuName switch
            {
                ShioSettings.ReservedGpuName_Default => DXGIGpuPreference.Unspecified,
                ShioSettings.ReservedGpuName_MinimumPower => DXGIGpuPreference.MinimumPower,
                ShioSettings.ReservedGpuName_HighPerformance => DXGIGpuPreference.HighPerformance,
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
        ShioUtils.ApplyWindowStyle(this, out _fixLagObject);
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
        InterlockedHelper.Write(ref _host, host);
        InterlockedHelper.Write(ref _collector, new DirtyAreaCollector(host));
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
        if (sender is not SimpleGraphicsHost host || !ReferenceEquals(host, InterlockedHelper.Read(ref _host)))
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

        RenderingController? controller = GetRenderingController();
        if (controller is null)
            return;
        controller.Lock();
        controller.WaitForRendering();
        Lock syncLock = _syncLock;
        syncLock.Enter();
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
            syncLock.Exit();
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
            RenderingController? controller = GetRenderingController();
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
            _syncLock.Exit();
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
    public DXGISwapChain GetSwapChain() => InterlockedHelper.Read(ref _host)!.GetSwapChain();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public D2D1DeviceContext GetDeviceContext() => NullSafetyHelper.ThrowIfNull(_deviceContext);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RenderingController? GetRenderingController() => InterlockedHelper.Read(ref _controller);

    public virtual void RenderBackground(UIElement element, in RegionalRenderingContext context)
        => context.Clear(_windowBaseColor);

    void IRenderable.Render(RenderingController controller)
    {
        RenderingFlags flags = controller.GetAndResetRenderingFlags();
        if (!flags.HasRenderRequest())
            return;
        if (!RenderCore(controller, flags))
            controller.RequestUpdateUnsafe(flags);
    }

    public IThemeResourceProvider? GetThemeResourceProvider() => InterlockedHelper.Read(ref _resourceProvider);

    IEnumerable<UIElement?> IElementContainer.GetActiveElements() => GetActiveElements();

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    IElementContainer IElementContainer.Parent => this;

    IRenderWindow IElementContainer.Window => this;

    CoreWindow IElementContainer.RootWindow => this;

    IThemeResourceProvider? IRenderWindow.GetDefaultThemeResourceProvider() => GetThemeResourceProvider();

    IThemeResourceProvider IRenderWindow.CreateThemeResourceProvider(IThemeContext context)
        => ThemeResourceProvider.CreateResourceProvider(this, context);

    Vector2 IRenderWindow.GetPixelsPerPoint() => _pixelsPerPoint;

    Vector2 IRenderWindow.GetPointsPerPixel() => _pointsPerPixel;

    Point IRenderWindow.InnerPageToPage(Point point) => PageToWindow(point);

    PointF IRenderWindow.InnerPageToPage(PointF point) => PageToWindow(point);

    Point IRenderWindow.PageToInnerPage(Point point) => WindowToPage(point);

    PointF IRenderWindow.PageToInnerPage(PointF point) => WindowToPage(point);

    RenderResult IRenderWindow.RenderPage(in RegionalRenderingContext context, in RenderInformation information)
        => NotSupportedException.Throw<RenderResult>();

    private bool IsBackgroundOpaque() => _actualWindowMaterial == WindowMaterial.None;
    #endregion

    #region Abstract Methods
    protected abstract void InitializeElements();

    protected abstract IEnumerable<UIElement?> GetActiveElements();
    #endregion

    #region Virtual Methods
    public virtual IEnumerable<UIElement?> GetElements() => GetActiveElements();

    protected virtual void ApplyThemeCore(IThemeResourceProvider provider)
    {
        _clearDCColor = provider.TryGetColor(ThemeConstants.ClearDCColorNode, out D2D1ColorF color) ? color : default;
        _windowBaseColor = provider.TryGetColor(ThemeConstants.WindowBaseColorNode, out color) ? color : default;
        UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
        ShioUtils.ResetBlur(this);

        UIElementHelper.ApplyTheme(provider, GetOverlayElement());
        ApplyThemeToElements(provider);
    }

    protected virtual void ApplyThemeToElements(IThemeResourceProvider provider)
        => UIElementHelper.ApplyTheme(provider, GetElements());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Point PageToWindow(Point point)
    {
        Point baseLoc = PageLocation;
        return new Point(baseLoc.X + point.X, baseLoc.Y + point.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual PointF PageToWindow(PointF point)
    {
        Point baseLoc = PageLocation;
        return new PointF(baseLoc.X + point.X, baseLoc.Y + point.Y);
    }

    public virtual Point WindowToPage(Point point)
    {
        Point baseLoc = PageLocation;
        return new Point(
            x: point.X - baseLoc.X,
            y: point.Y - baseLoc.Y
            );
    }

    public virtual PointF WindowToPage(PointF point)
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

    protected virtual void OnMouseDownForElements(ref HandleableMouseEventArgs args, ref HitTestData data)
    {
        UIElement? overlayElement = GetOverlayElement();
        if (args.Handled)
        {
            ref readonly MouseEventArgs readonlyArgs = ref UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args);
            if (overlayElement is not null)
                UIElementHelper.OnGlobalMouseDownForElement(overlayElement, in readonlyArgs);
            else
                UIElementHelper.OnGlobalMouseDownForElements(GetActiveElements(), in readonlyArgs);
        }
        else
        {
            if (overlayElement is not null)
                UIElementHelper.OnMouseDownForElement(overlayElement, ref args, ref data);
            else
                UIElementHelper.OnMouseDownForElements(GetActiveElements(), ref args, ref data);
        }
    }

    protected virtual void OnMouseMoveForElements(in MouseEventArgs args, ref MouseMoveData data)
    {
        UIElement? overlayElement = GetOverlayElement();
        if (overlayElement is not null)
            UIElementHelper.OnMouseMoveForElement(overlayElement, args, ref data);
        else
            UIElementHelper.OnMouseMoveForElements(GetActiveElements(), args, ref data);
    }

    protected virtual void OnMouseUpForElements(in MouseEventArgs args, ref MouseUpData data)
    {
        UIElement? overlayElement = GetOverlayElement();
        if (data.HitElements.Count <= 0)
        {
            if (overlayElement is not null)
                UIElementHelper.OnGlobalMouseUpForElement(overlayElement, in args);
            else
                UIElementHelper.OnGlobalMouseUpForElements(GetActiveElements(), in args);
        }
        else
        {
            if (overlayElement is not null)
                UIElementHelper.OnMouseUpForElement(overlayElement, in args, ref data);
            else
                UIElementHelper.OnMouseUpForElements(GetActiveElements(), in args, ref data);
        }
    }

    protected virtual void OnMouseScrollForElements(ref HandleableMouseEventArgs args, ref HitTestData data)
    {
        UIElement? overlayElement = GetOverlayElement();
        if (args.Handled)
        {
            ref readonly MouseEventArgs readonlyArgs = ref UnsafeHelper.As<HandleableMouseEventArgs, MouseEventArgs>(ref args);
            if (overlayElement is not null)
                UIElementHelper.OnGlobalMouseScrollForElement(overlayElement, in readonlyArgs);
            else
                UIElementHelper.OnGlobalMouseScrollForElements(GetActiveElements(), in readonlyArgs);
        }
        else
        {
            if (overlayElement is not null)
                UIElementHelper.OnMouseScrollForElement(overlayElement, ref args, ref data);
            else
                UIElementHelper.OnMouseScrollForElements(GetActiveElements(), ref args, ref data);
        }
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

    protected unsafe virtual void RecalculateLayout(ref WindowLayoutData data, Size windowSize)
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

    protected virtual void RecalculatePageLayout(Size pageSize, in RecalculateLayoutInformation information)
    {
        using LayoutEngineRentScope engine = LayoutEngine.Rent();
        engine.RecalculateLayout(pageSize, GetActiveElements(), information);
        engine.RecalculateLayout(pageSize, GetOverlayElement(), information);
        Thread.MemoryBarrier();
    }
    #endregion

    #region Rendering
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected UIElement? GetFocusedElement()
    {
        lock (_syncLock)
        {
            GCHandle elementRef = _focusElementRef;
            if (elementRef.IsAllocated && elementRef.Target is UIElement element)
                return element;
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected UIElement? GetOverlayElement()
    {
        lock (_syncLock)
            return _overlayElement;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected D2D1Brush GetBrush(Brush brush)
    {
        if (brush >= Brush._Last)
            ArgumentOutOfRangeException.Throw(nameof(brush));
        return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)brush);
    }

    [Inline]
    private bool RenderCore(RenderingController controller, RenderingFlags flags)
    {
        SimpleGraphicsHost? host = InterlockedHelper.Read(ref _host);
        if (host is null || host.IsDisposed)
            return false;
        DirtyAreaCollector? collector = InterlockedHelper.Read(ref _collector);
        if (collector is null)
            return false;

        ulong renderTimestamp = NativeMethods.GetTicksForSystem();
        WindowRenderingData data = new()
        {
            Layout = new()
            {
                MinimizeButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_minimizeButtonLocation, _minimizeButtonSize),
                MaximizeButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_maximizeButtonLocation, _maximizeButtonSize),
                CloseButtonBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_closeButtonLocation, _closeButtonSize),
                PageBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_pageLocation, _pageSize),
                TitleBarBounds = BoundsHelper.ConvertUInt64SlotsToBounds(_titleBarLocation, _titleBarSize),
                DrawingOffset = _drawingOffset,
                ActiveBorderWidth = _activeBorderWidth
            },
            ResizeTimestamp = _resizeTimestamp,
            LastRenderTimestamp = ReferenceHelper.Exchange(ref _renderTimestamp, renderTimestamp),
            CurrentRenderTimestamp = renderTimestamp
        };
        bool renderAll = flags.HasRedrawAll();
        if (flags.HasResize())
        {
            if (!TryResize(host, controller, ref data, flags, clearCache: false, ref renderAll))
                return false;
            if (host is OptimizedGraphicsHost optimizedHost)
                renderAll |= optimizedHost.ForcePresentAll;
        }
        D2D1DeviceContext? deviceContext = host.BeginDraw();
        if (deviceContext is null || deviceContext.IsDisposed)
            return true;

        (bool Presented, RenderResult ResultFlags) result;
        ClearTypeSwitcher.SetClearType(deviceContext, false);
        if (renderAll)
            result = Render_All(host, deviceContext, in data);
        else
            result = Render_Incremental(host, deviceContext, collector, in data);

        RenderResult resultFlags = result.ResultFlags;
        if (!resultFlags.IsSuccessed())
        {
            Size pageSize = data.Layout.PageBounds.Size;
            if (!pageSize.IsValid())
                return false;

            bool debugMode = ShioSettings.UseDebugMode && Debugger.IsAttached;
            nuint renderDesyncTimes = 0, layoutDesyncTimes = 0, retryTimes = 0;
            do
            {
                if (resultFlags >= RenderResult.LayoutDesync)
                {
                    if (debugMode)
                    {
                        renderDesyncTimes = 0;
                        if (++layoutDesyncTimes > 3)
                        {
                            layoutDesyncTimes = 3;
                            Debugger.Log(level: 1, "UI Rendering warning",
                                $"Thread {NativeMethods.GetCurrentThreadId()}(Name = {Thread.CurrentThread.Name}) is re-rendering in LayoutDesync state for this frame over 3 times! (Timestamp = ${data.CurrentRenderTimestamp})");
                        }
                    }

                    flags = controller.GetAndResetRenderingFlags(); // 反正都要二次重繪，那就在拉取一次最新的渲染旗標並重置，以減少過度渲染的機會
                    if (flags.HasResize())
                    {
                        if (!TryResize(host, controller, ref data, flags, clearCache: true, ref renderAll) || !(pageSize = data.Layout.PageBounds.Size).IsValid())
                            return false;
                    }
                    else
                    {
                        RecalculatePageLayout(pageSize, new (data.ResizeTimestamp, clearCache: true));
                    }
                }
                else
                {
                    DebugHelper.ThrowIf(resultFlags != RenderResult.RenderDesync);
                    if (debugMode)
                    {
                        layoutDesyncTimes = 0;
                        if (++renderDesyncTimes > 3)
                        {
                            renderDesyncTimes = 3;
                            Debugger.Log(level: 1, "UI Rendering warning",
                                $"Thread {NativeMethods.GetCurrentThreadId()}(Name = {Thread.CurrentThread.Name}) is re-rendering in RenderDesync state for this frame over 3 times! (Timestamp = ${data.CurrentRenderTimestamp})");
                        }
                    }
                }
                if (debugMode && ++retryTimes > 3 && layoutDesyncTimes <= 1 && renderDesyncTimes <= 1)
                {
                    retryTimes = 3;
                    Debugger.Log(level: 1, "UI Rendering warning",
                        $"Thread {NativeMethods.GetCurrentThreadId()}(Name = {Thread.CurrentThread.Name}) is re-rendering for this frame over 3 times! (Timestamp = ${data.CurrentRenderTimestamp})");
                }

                deviceContext = host.BeginDraw();
                if (deviceContext is null || deviceContext.IsDisposed)
                    return true;

                ClearTypeSwitcher.SetClearType(deviceContext, false);
                result = Render_All(host, deviceContext, in data);
                resultFlags = result.ResultFlags;
            } while (!resultFlags.IsSuccessed());
        }

        return result.Presented;

        bool TryResize(SimpleGraphicsHost host, RenderingController controller, ref WindowRenderingData data, RenderingFlags flags, bool clearCache, ref bool renderAll)
        {
            bool resizeTemporarily = flags.HasResizeTemporarily();
            Size clirentSizeInPixel = RawClientSize;
            if (clirentSizeInPixel.Width <= 0 || clirentSizeInPixel.Height <= 0)
                return false;
            if (resizeTemporarily)
                renderAll |= host.ResizeTemporarily(clirentSizeInPixel);
            else
                renderAll |= host.Resize(clirentSizeInPixel);
            Thread.MemoryBarrier();
            if (renderAll)
            {
                data.ResizeTimestamp = data.CurrentRenderTimestamp;

                ref WindowLayoutData layoutData = ref data.Layout;
                RecalculateLayout(
                    data: ref layoutData,
                    windowSize: GraphicsUtils.ScalingSizeAndConvert(clirentSizeInPixel, _pointsPerPixel));
                BoundsHelper.SaveBoundsToUInt64Fields(layoutData.MinimizeButtonBounds, ref _minimizeButtonLocation, ref _minimizeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(layoutData.MaximizeButtonBounds, ref _maximizeButtonLocation, ref _maximizeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(layoutData.CloseButtonBounds, ref _closeButtonLocation, ref _closeButtonSize);
                BoundsHelper.SaveBoundsToUInt64Fields(layoutData.PageBounds, ref _pageLocation, ref _pageSize);
                BoundsHelper.SaveBoundsToUInt64Fields(layoutData.TitleBarBounds, ref _titleBarLocation, ref _titleBarSize);

                ulong timestamp = data.ResizeTimestamp;
                _resizeTimestamp = timestamp;
                _drawingOffset = layoutData.DrawingOffset;
                _activeBorderWidth = layoutData.ActiveBorderWidth;
                InterlockedHelper.Increment(ref _recalculateLayoutVersion);

                Size pageSize = layoutData.PageBounds.Size;
                if (pageSize.IsValid())
                    RecalculatePageLayout(pageSize, new(data.ResizeTimestamp, clearCache: clearCache));
            }
            flags = controller.GetAndResetRenderingFlags();
            renderAll |= flags.HasRedrawAll();
            if (resizeTemporarily || flags.HasResize())
                controller.RequestUpdateAndResize(flags.HasResizeTemporarily(), redrawAll: false);
            return true;
        }

        (bool Presented, RenderResult ResultFlags) Render_All(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, in WindowRenderingData data)
        {
            RenderResult result = RenderResult.Successed;
            try
            {
                DirtyAreaCollector collector = DirtyAreaCollector.Empty;
                RenderTitle(deviceContext, collector, force: true, in data);
                Rectangle pageBounds = data.Layout.PageBounds;
                if (pageBounds.IsValid())
                {
                    using RegionalRenderingContext context = RegionalRenderingContext.Create(deviceContext, collector, _pixelsPerPoint,
                        pageBounds, D2D1AntialiasMode.Aliased, IsBackgroundOpaque(), out _);
                    result = RenderPage(context, in data);
                }
            }
            finally
            {
                host.EndDraw();
            }

            return (Presented: result.IsSuccessed() && host.TryPresent(), ResultFlags: result);
        }

        (bool Presented, RenderResult ResultFlags) Render_Incremental(SimpleGraphicsHost host, D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in WindowRenderingData data)
        {
            Vector2 pixelsPerPoint = _pixelsPerPoint;
            RenderResult result = RenderResult.Successed;
            try
            {
                RenderTitle(deviceContext, collector, force: false, in data);
                Rectangle pageBounds = data.Layout.PageBounds;
                if (pageBounds.IsValid())
                {
                    using RegionalRenderingContext context = RegionalRenderingContext.Create(deviceContext, collector, pixelsPerPoint,
                        pageBounds, D2D1AntialiasMode.Aliased, IsBackgroundOpaque(), out _);
                    result = RenderPage(context, in data);
                }
            }
            finally
            {
                host.EndDraw();
            }

            if (result.IsSuccessed())
                return (Presented: collector.TryPresent(pixelsPerPoint), ResultFlags: RenderResult.Successed);

            collector.Clear();
            return (Presented: false, ResultFlags: result);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void RenderPageBackground(in RegionalRenderingContext context, in WindowRenderingData data)
        => context.Clear(_windowBaseColor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual RenderResult RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
    {
        bool force = context.IsForceRendering;
        if (force)
            RenderPageBackground(context, in data);

        RenderResult result = UIElementHelper.RenderElements(context, GetActiveElements(), data.CreateRenderInformation(force));
        if (result.ShouldImmediatelyReturn())
            return result;
        return result | UIElementHelper.RenderElement(context, _overlayElement, data.CreateRenderInformation(ignoreNeedRefresh: force || context.HasAnyDirtyArea()));
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
                RectF titleBarRect = RenderingHelper.RoundInPixel(data.Layout.TitleBarBounds, pixelsPerPoint);
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
                RectF minRect = RenderingHelper.RoundInPixel(data.Layout.MinimizeButtonBounds, pixelsPerPoint);
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
                RectF maxRect = RenderingHelper.RoundInPixel(data.Layout.MaximizeButtonBounds, pixelsPerPoint);
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
            RectF closeRect = RenderingHelper.RoundInPixel(data.Layout.CloseButtonBounds, pixelsPerPoint);
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
        => GetRenderingController()?.Lock();

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        RenderingController? controller = GetRenderingController();
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
                GetRenderingController()?.Lock();
                break;
            case SessionSwitchReason.SessionUnlock:
                {
                    RenderingController? controller = GetRenderingController();
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
                GetRenderingController()?.Lock();
                break;
            case PowerModes.Resume:
                {
                    RenderingController? controller = GetRenderingController();
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
        ShioUtils.ResetBlur(this);
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
        InterlockedHelper.Write(ref _controller, controller);
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
    private static BatchUpdateScope EnterBatchUpdateScopeCore(RenderingController? controller)
    {
        if (controller is null)
            return default;
        return new BatchUpdateScope(controller);
    }

    [Inline(InlineBehavior.Remove)]
    private static CriticalUpdateScope EnterCriticalUpdateScopeCore(RenderingController? controller)
    {
        if (controller is null)
            return default;
        return new CriticalUpdateScope(controller);
    }

    [Inline(InlineBehavior.Remove)]
    private void ChangeDpi_RenderingPart(PointU dpi, Vector2 pointsPerPixel, Vector2 pixelsPerPoint)
    {
        SimpleGraphicsHost? host = InterlockedHelper.Read(ref _host);
        if (host is null || host.IsDisposed)
            return;
        RenderingController? controller = GetRenderingController();
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
        RenderingController? controller = GetRenderingController();
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
    public BatchUpdateScope EnterBatchUpdateScope() => EnterBatchUpdateScopeCore(GetRenderingController());

    public CriticalUpdateScope EnterCriticalUpdateScope() => EnterCriticalUpdateScopeCore(GetRenderingController());

    public void UpdateAndResize() => UpdateAndResizeCore(GetRenderingController(), ref _sizeModeState);

    public void Update() => UpdateCore(GetRenderingController());

    public void Refresh() => RefreshCore(GetRenderingController());

    public void ChangeFocusElement(UIElement? element)
    {
        if (element is not IFocusChangedHandler handler)
        {
            lock (_syncLock)
                ClearFocusElementCore();
        }
        else
        {
            lock (_syncLock)
                ChangeFocusElementCore(element, handler);
        }
    }

    protected void ClearFocusElement()
    {
        lock (_syncLock)
            ClearFocusElementCore();
    }

    public void ClearFocusElement(UIElement? elementForValidation)
    {
        lock (_syncLock)
            ClearFocusElementCore(elementForValidation);
    }

    private void ChangeFocusElementCore(UIElement element, IFocusChangedHandler handler)
    {
        ref GCHandle elementRef = ref _focusElementRef;
        if (elementRef.IsAllocated)
        {
            object? target = elementRef.Target;
            if (!ReferenceEquals(element, target) && target is IFocusChangedHandler oldHandler)
                oldHandler.OnFocusChanged(new FocusChangedEventArgs(State: false, FocusedElement: null));
            handler.OnFocusChanged(new FocusChangedEventArgs(State: true, FocusedElement: element));
            elementRef.Target = element;
        }
        else
        {
            handler.OnFocusChanged(new FocusChangedEventArgs(State: true, FocusedElement: element));
            elementRef = GCHandle.Alloc(element, GCHandleType.Weak);
        }
    }

    private void ClearFocusElementCore()
    {
        ref GCHandle elementRef = ref _focusElementRef;
        if (elementRef.IsAllocated)
        {
            object? target = elementRef.Target;
            if (target is IFocusChangedHandler oldHandler)
                oldHandler.OnFocusChanged(new FocusChangedEventArgs(State: false, FocusedElement: null));
            elementRef.Target = null;
        }
    }

    private void ClearFocusElementCore(UIElement? elementForValidation)
    {
        ref GCHandle elementRef = ref _focusElementRef;
        if (elementRef.IsAllocated)
        {
            object? target = elementRef.Target;
            if (!ReferenceEquals(target, elementForValidation))
                return;

            if (target is IFocusChangedHandler oldHandler)
                oldHandler.OnFocusChanged(new FocusChangedEventArgs(State: false, FocusedElement: null));
            elementRef.Target = null;
        }
    }

    private void ChangeLastMouseDownHitElement(UIElement? element, MouseButtons buttons)
    {
        if (element is null)
        {
            lock (_syncLock)
                ClearCore(buttons);
        }
        else
        {
            lock (_syncLock)
                ChangeCore(element, buttons);
        }

        void ChangeCore(UIElement element, MouseButtons buttons)
        {
            ref GCHandle recordedHitElementArrayRef = ref UnsafeHelper.GetArrayDataReference(_recordedMouseDownHitElementRefs);
            if (buttons.HasFlagFast((MouseButtons)0b0000001))
                ChangeTarget(ref recordedHitElementArrayRef, element);
            if (buttons.HasFlagFast((MouseButtons)0b0000010))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 1), element);
            if (buttons.HasFlagFast((MouseButtons)0b0000100))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 2), element);
            if (buttons.HasFlagFast((MouseButtons)0b0001000))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 3), element);
            if (buttons.HasFlagFast((MouseButtons)0b0010000))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 4), element);
            if (buttons.HasFlagFast((MouseButtons)0b0100000))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 5), element);
            if (buttons.HasFlagFast((MouseButtons)0b1000000))
                ChangeTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 6), element);
        }

        void ClearCore(MouseButtons buttons)
        {
            ref GCHandle recordedHitElementArrayRef = ref UnsafeHelper.GetArrayDataReference(_recordedMouseDownHitElementRefs);
            if (buttons.HasFlagFast((MouseButtons)0b0000001))
                ClearTarget(ref recordedHitElementArrayRef);
            if (buttons.HasFlagFast((MouseButtons)0b0000010))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 1));
            if (buttons.HasFlagFast((MouseButtons)0b0000100))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 2));
            if (buttons.HasFlagFast((MouseButtons)0b0001000))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 3));
            if (buttons.HasFlagFast((MouseButtons)0b0010000))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 4));
            if (buttons.HasFlagFast((MouseButtons)0b0100000))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 5));
            if (buttons.HasFlagFast((MouseButtons)0b1000000))
                ClearTarget(ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 6));
        }

        static void ChangeTarget(ref GCHandle handle, UIElement element)
        {
            if (handle.IsAllocated)
                handle.Target = element;
            else
                handle = GCHandle.Alloc(element, GCHandleType.Weak);
        }

        static void ClearTarget(ref GCHandle handle)
        {
            if (handle.IsAllocated)
                handle.Target = null;
        }
    }

    private void GetAndClearLastMouseDownHitElements(PooledList<UIElement> list, MouseButtons buttons)
    {
        lock (_syncLock)
        {
            ref GCHandle recordedHitElementArrayRef = ref UnsafeHelper.GetArrayDataReference(_recordedMouseDownHitElementRefs);
            if (buttons.HasFlagFast((MouseButtons)0b0000001))
                GetAndClearTarget(list, ref recordedHitElementArrayRef);
            if (buttons.HasFlagFast((MouseButtons)0b0000010))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 1));
            if (buttons.HasFlagFast((MouseButtons)0b0000100))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 2));
            if (buttons.HasFlagFast((MouseButtons)0b0001000))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 3));
            if (buttons.HasFlagFast((MouseButtons)0b0010000))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 4));
            if (buttons.HasFlagFast((MouseButtons)0b0100000))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 5));
            if (buttons.HasFlagFast((MouseButtons)0b1000000))
                GetAndClearTarget(list, ref UnsafeHelper.AddTypedOffset(ref recordedHitElementArrayRef, 6));
        }

        static void GetAndClearTarget(PooledList<UIElement> list, ref GCHandle handle)
        {
            if (handle.IsAllocated)
            {
                if (handle.Target is UIElement target && !list.Contains(target))
                    list.Add(target);
                handle.Target = null;
            }
        }
    }

    private void ChangeLastMouseMoveHitElement(UIElement? element, in MouseEventArgs args)
    {
        if (element is null)
        {
            lock (_syncLock)
                ClearCore(args);
        }
        else
        {
            lock (_syncLock)
                ChangeCore(element, args);
        }

        void ChangeCore(UIElement element, in MouseEventArgs args)
        {
            ref GCHandle elementRef = ref _lastMouseMoveHitElementRef;
            if (elementRef.IsAllocated)
            {
                object? target = elementRef.Target;
                if (!ReferenceEquals(element, target) && target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                    handler.OnMouseMove(new MouseEventArgs(lastHitElement.GlobalPageToLocalPage(lastHitElement.PageToLocal(args.Location)), args.Buttons, args.Delta));
                elementRef.Target = element;
            }
            else
            {
                elementRef = GCHandle.Alloc(element, GCHandleType.Weak);
            }
        }

        void ClearCore(in MouseEventArgs args)
        {
            ref GCHandle elementRef = ref _lastMouseMoveHitElementRef;
            if (elementRef.IsAllocated)
            {
                object? target = elementRef.Target;
                if (target is UIElement lastHitElement && target is IMouseMoveHandler handler)
                    handler.OnMouseMove(new MouseEventArgs(lastHitElement.GlobalPageToLocalPage(lastHitElement.PageToLocal(args.Location)), args.Buttons, args.Delta));
                elementRef.Target = null;
            }
        }
    }

    public UIElement? ChangeOverlayElement(UIElement? element)
    {
        using BatchUpdateScope scope = EnterBatchUpdateScope();
        UIElement? oldElement;
        lock (_syncLock)
        {
            oldElement = ReferenceHelper.Exchange(ref _overlayElement, element);

            OnOverlayLayerChanged(element, oldElement);
        }
        UpdateAndResize();
        return oldElement;
    }

    public void ChangeOverlayElement(UIElement? element, UIElement? oldElement)
    {
        using BatchUpdateScope scope = EnterBatchUpdateScope();
        lock (_syncLock)
        {
            if (!ReferenceEquals(_overlayElement, oldElement))
                return;
            _overlayElement = element;

            OnOverlayLayerChanged(element, oldElement);
        }
        UpdateAndResize();
    }

    private void OnOverlayLayerChanged(UIElement? element, UIElement? oldElement)
    {
        ClearFocusElementCore();
        if (element is null)
        {
            ref GCHandle recordedElementRef = ref _recordedLastMouseMoveHitElementRef;
            if (recordedElementRef.IsAllocated && recordedElementRef.Target is object target)
            {
                ref GCHandle elementRef = ref _lastMouseMoveHitElementRef;
                if (elementRef.IsAllocated)
                    elementRef.Target = target;
                else
                    elementRef = GCHandle.Alloc(target, GCHandleType.Weak);
                recordedElementRef.Target = null;
            }
        }
        else if (oldElement is null)
        {
            ref GCHandle elementRef = ref _lastMouseMoveHitElementRef;
            if (elementRef.IsAllocated && elementRef.Target is object target)
            {
                ref GCHandle recordedElementRef = ref _recordedLastMouseMoveHitElementRef;
                if (recordedElementRef.IsAllocated)
                    recordedElementRef.Target = target;
                else
                    recordedElementRef = GCHandle.Alloc(target, GCHandleType.Weak);
            }
        }
        OnMouseMove(new MouseEventArgs(PointToClient(MouseHelper.GetMousePosition())));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CloseOverlayElement(UIElement elementForValidate)
        => ChangeOverlayElement(null, elementForValidate);

    protected unsafe void ApplyTheme(IThemeResourceProvider provider)
    {
        RenderingController? controller = GetRenderingController();
        if (controller is not null)
        {
            controller.Lock();
            controller.WaitForRendering();
        }
        try
        {
            lock (_syncLock)
            {
                SimpleGraphicsHost? host = InterlockedHelper.Read(ref _host);
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
    protected virtual void DisposeAllElements()
        => UIElementHelper.DisposeForElements(GetElements());

    private static void SafeDispose(ref GCHandle handle)
    {
        if (handle.IsAllocated)
            handle.Free();
    }

    protected override void DisposeCore(bool disposing)
    {
        if (disposing)
        {
            DisposeHelper.SwapDisposeInterlockedWeak(ref _resourceProvider);
            DisposeHelper.SwapDisposeInterlocked(ref _controller);
            DisposeHelper.SwapDisposeInterlocked(ref _host);
            DisposeHelper.SwapDisposeInterlocked(ref _titleLayout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            GetOverlayElement()?.Dispose();
            DisposeAllElements();

            if (InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                SpinWait.SpinUntil(() => InterlockedHelper.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0);
            if (InterlockedHelper.Read(ref _ownedGDP) != 0)
                DisposeHelper.SwapDisposeInterlocked(ref _graphicsDeviceProvider);
            else
                InterlockedHelper.Write(ref _graphicsDeviceProvider, null);
        }
        _overlayElement = null;
        SafeDispose(ref _recordedLastMouseMoveHitElementRef);
        SafeDispose(ref _lastMouseMoveHitElementRef);
        SafeDispose(ref _focusElementRef);
        SequenceHelper.Clear(_brushes);
        base.DisposeCore(disposing);
    }
    #endregion
}
