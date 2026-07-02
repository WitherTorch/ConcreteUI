using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using RiceTea.Core.Buffers;
using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class CoreWindow : NativeWindow
{
    #region Static Fields
    private static readonly UnwrappableList<GCHandle> _rootWindowList = new UnwrappableList<GCHandle>();
    #endregion

    #region Fields
    private readonly UnwrappableList<GCHandle> _childrenReferenceList = new UnwrappableList<GCHandle>();
    private readonly CoreWindow? _parent;
    private PointU _dpi = SystemConstants.DefaultDpi;
    private Vector2 _pixelsPerPoint = Vector2.One; // 螢幕DPI / 96
    private Vector2 _pointsPerPixel = Vector2.One; //  96 / 螢幕DPI
    private BitVector64 _titleBarStates = ulong.MaxValue;
    private bool _isIntegratedMaterial = false;
    #endregion

    #region Events
    public event EventHandler? DpiChanged;
    #endregion

    #region Event Triggers
    protected override void OnWindowStateChanged(in WindowStateChangedEventArgs args)
    {
        base.OnWindowStateChanged(args);
        OnWindowStateChangedRenderingPart(args);
    }

    protected virtual void OnDpiChanged() => DpiChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnMouseDown(ref HandleableMouseEventArgs args)
    {
        HitTestData data = default;
        HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(WindowToPage(args.Location), args.Buttons, args.Delta);
        try
        {
            OnMouseDownForElements(ref relativeArgs, ref data);
            if (relativeArgs.Handled)
                args.Handle();
        }
        finally
        {
            ChangeLastMouseDownHitElement(data.LastHitElement, args.Buttons);
            ChangeFocusElement(data.LastHitElement);
        }
    }

    protected virtual void OnMouseUp(in MouseEventArgs args)
    {
        using PooledList<UIElement> list = new(capacity: 0);
        GetAndClearLastMouseDownHitElements(list, args.Buttons);
        MouseUpData data = new MouseUpData(list);
        OnMouseUpForElements(new MouseEventArgs(WindowToPage(args.Location), args.Buttons, args.Delta), ref data);
    }

    protected virtual void OnMouseMove(in MouseEventArgs args)
    {
        MouseMoveData data = default;
        try
        {
            OnMouseMoveForElements(new MouseEventArgs(WindowToPage(args.Location), args.Buttons, args.Delta), ref data);
        }
        finally
        {
            Cursor = SystemCursors.GetSystemCursor(data.CursorType ?? SystemCursorType.Default);
            ChangeLastMouseMoveHitElement(data.LastHitElement, args);
        }
    }

    protected virtual void OnMouseScroll(ref HandleableMouseEventArgs args)
    {
        HitTestData data = default;
        HandleableMouseEventArgs relativeArgs = new HandleableMouseEventArgs(WindowToPage(args.Location), args.Buttons, args.Delta);
        try
        {
            OnMouseScrollForElements(ref relativeArgs, ref data);
        }
        finally
        {
            if (relativeArgs.Handled)
                args.Handle();
        }
    }

    protected virtual void OnKeyDown(ref KeyEventArgs args)
        => OnKeyDownForElements(ref args);

    protected virtual void OnKeyUp(ref KeyEventArgs args)
        => OnKeyUpForElements(ref args);
    #endregion

    #region Properties
    public CoreWindow? Parent => _parent;
    public IThemeContext? CurrentTheme => _resourceProvider?.ThemeContext;
    public PointU Dpi => _dpi;
    public Vector2 PixelsPerPoint => _pixelsPerPoint;
    public Vector2 PointsPerPixel => _pointsPerPixel;

    public new Rectangle Bounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Rectangle)GraphicsUtils.ScalingRect(base.Bounds, _pointsPerPixel);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => base.Bounds = (Rectangle)GraphicsUtils.ScalingRect(value, _pixelsPerPoint);
    }

    public new Rectangle ClientBounds => (Rectangle)GraphicsUtils.ScalingRect(base.ClientBounds, _pointsPerPixel);
    public new Point Location => Bounds.Location;
    public new Size Size => Bounds.Size;
    public new Size ClientSize => ClientBounds.Size;
    public new int X => Bounds.X;
    public new int Y => Bounds.Y;
    public new int Width => Bounds.Width;
    public new int Height => Bounds.Height;

    public Rectangle RawBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => base.Bounds;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => base.Bounds = value;
    }

    public Rectangle RawClientBounds => base.ClientBounds;
    public Point RawLocation => base.Location;
    public Size RawSize => base.Size;
    public Size RawClientSize => base.ClientSize;
    public int RawX => base.X;
    public int RawY => base.Y;
    public int RawWidth => base.Width;
    public int RawHeight => base.Height;
    public bool MaximizeBox
    {
        get
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return false;

                const int GWL_STYLE = -16;
                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                return (styles & WindowStyles.MaximizeBox) == WindowStyles.MaximizeBox;
            }
            else
            {
                return _titleBarButtonStatus[2];
            }
        }
        set
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                const int GWL_STYLE = -16;

                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                if (value)
                    styles |= WindowStyles.MaximizeBox;
                else
                    styles &= ~WindowStyles.MaximizeBox;
                User32.SetWindowLongPtrW(handle, GWL_STYLE, (nint)styles);
            }
            else
            {
                bool state = _titleBarStates[2];
                if (state == value)
                    return;
                _titleBarStates[2] = value;

                Update();
            }
        }
    }

    public bool MinimizeBox
    {
        get
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return false;

                const int GWL_STYLE = -16;
                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                return (styles & WindowStyles.MinimizeBox) == WindowStyles.MinimizeBox;
            }
            else
            {
                return _titleBarButtonStatus[1];
            }
        }
        set
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                const int GWL_STYLE = -16;

                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                if (value)
                    styles |= WindowStyles.MinimizeBox;
                else
                    styles &= ~WindowStyles.MinimizeBox;
                User32.SetWindowLongPtrW(handle, GWL_STYLE, (nint)styles);
            }
            else
            {
                bool state = _titleBarStates[1];
                if (state == value)
                    return;
                _titleBarStates[1] = value;

                Update();
            }
        }
    }

    public bool ShowTitle
    {
        get
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return false;

                const int GWL_STYLE = -16;
                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                return (styles & WindowStyles.Caption) == WindowStyles.Caption;
            }
            else
            {
                return _titleBarButtonStatus[0];
            }
        }
        set
        {
            if (_isIntegratedMaterial)
            {
                IntPtr handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
                const int GWL_STYLE = -16;

                WindowStyles styles = (WindowStyles)User32.GetWindowLongPtrW(handle, GWL_STYLE);
                if (value)
                    styles |= WindowStyles.Caption;
                else
                    styles &= ~WindowStyles.Caption;
                User32.SetWindowLongPtrW(handle, GWL_STYLE, (nint)styles);
            }
            else
            {
                bool state = _titleBarStates[0];
                if (state == value)
                    return;
                _titleBarStates[0] = value;

                Update();
            }
        }
    }
    #endregion
    protected unsafe CoreWindow() : this(deviceProvider: null)
    {
        _parent = null;
        _activeElementsCacheStore = new(this, &CreateSnapshotForActiveElements, &DropSnapshot);
        _elementsCacheStore = new(this, &CreateSnapshotForElements, &DropSnapshot);
    }

    protected unsafe CoreWindow(GraphicsDeviceProvider? deviceProvider) : base(null)
    {
        _parent = null;
        _activeElementsCacheStore = new(this, &CreateSnapshotForActiveElements, &DropSnapshot);
        _elementsCacheStore = new(this, &CreateSnapshotForElements, &DropSnapshot);

        _graphicsDeviceProvider = deviceProvider;
        _windowMaterial = ShioSettings.WindowMaterial;
        UnwrappableList<GCHandle> windowList = _rootWindowList;
        lock (windowList)
            windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
        InitUnmanagedPart();
    }

    protected unsafe CoreWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(passParentToUnderlyingWindow ? parent : null)
    {
        _parent = parent;
        _activeElementsCacheStore = new(this, &CreateSnapshotForActiveElements, &DropSnapshot);
        _elementsCacheStore = new(this, &CreateSnapshotForElements, &DropSnapshot);

        UnwrappableList<GCHandle> windowList;
        if (parent is null)
        {
            _graphicsDeviceProvider = null;
            _windowMaterial = ShioSettings.WindowMaterial;
            windowList = _rootWindowList;
        }
        else
        {
            _graphicsDeviceProvider = parent.GetGraphicsDeviceProvider();
            _windowMaterial = parent.WindowMaterial;
            windowList = parent._childrenReferenceList;
        }
        lock (windowList)
            windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
        InitUnmanagedPart();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIntegratedMaterial(WindowMaterial material)
    {
        switch (material)
        {
            case WindowMaterial.Integrated:
                return true;
            case WindowMaterial.Default:
                return SystemHelper.GetDefaultMaterial() == WindowMaterial.Integrated;
            default:
                if (SequenceHelper.Contains(SystemHelper.GetAvailableMaterials(), material))
                    return false;
                goto case WindowMaterial.Default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async void InvokeUpdateWindowFps(CoreWindow window)
    {
        await Task.Delay(2000);
        IntPtr handle = window.Handle;
        if (handle == IntPtr.Zero)
            return;
        User32.PostMessageW(handle, CustomWindowMessages.ShioUpdateRefreshRate, 0, 0);
    }
}
