using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Window
{
    public abstract partial class CoreWindow : NativeWindow
    {
        #region Static Fields
        private static readonly UnwrappableList<GCHandle> _rootWindowList = new UnwrappableList<GCHandle>();
        private static readonly Func<WeakReference> _weakReferenceFactory = static () => new WeakReference(null);
        #endregion

        #region Fields
        private readonly UnwrappableList<GCHandle> _childrenReferenceList = new UnwrappableList<GCHandle>();
        private readonly CoreWindow? _parent;
        private PointU _dpi = SystemConstants.DefaultDpi;
        private Vector2 _pixelsPerPoint = Vector2.One; // 螢幕DPI / 96
        private Vector2 _pointsPerPixel = Vector2.One; //  96 / 螢幕DPI
        private BitVector64 _titleBarStates = ulong.MaxValue;
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
            => OnMouseDownForElements(ref args);

        protected virtual void OnMouseUp(in MouseEventArgs args)
            => OnMouseUpForElements(in args);

        protected virtual void OnMouseMove(in MouseEventArgs args)
            => OnMouseMoveForElements(in args);

        protected virtual void OnMouseScroll(ref HandleableMouseEventArgs args)
            => OnMouseScrollForElements(ref args);

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
                if (_windowMaterial == WindowMaterial.Integrated)
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
                if (_windowMaterial == WindowMaterial.Integrated)
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
                if (_windowMaterial == WindowMaterial.Integrated)
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
                if (_windowMaterial == WindowMaterial.Integrated)
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
                if (_windowMaterial == WindowMaterial.Integrated)
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
                if (_windowMaterial == WindowMaterial.Integrated)
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
        protected CoreWindow() : this(deviceProvider: null) { }

        protected CoreWindow(GraphicsDeviceProvider? deviceProvider) : base(null)
        {
            _parent = null;
            Func<WeakReference> weakReferenceFactory = _weakReferenceFactory;
            _focusElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.PublicationOnly);
            _recordedLastHitElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.PublicationOnly);
            _lastHitElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.None);
            _graphicsDeviceProvider = deviceProvider;
            _windowMaterial = GetRealWindowMaterial(ConcreteSettings.WindowMaterial);
            UnwrappableList<GCHandle> windowList = _rootWindowList;
            lock (windowList)
                windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
            InitUnmanagedPart();
        }

        protected CoreWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(passParentToUnderlyingWindow ? parent : null)
        {
            _parent = parent;
            Func<WeakReference> weakReferenceFactory = _weakReferenceFactory;
            _focusElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.PublicationOnly);
            _recordedLastHitElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.PublicationOnly);
            _lastHitElementRefLazy = new LazyTiny<WeakReference>(weakReferenceFactory, LazyThreadSafetyMode.None);
            UnwrappableList<GCHandle> windowList;
            if (parent is null)
            {
                _graphicsDeviceProvider = null;
                _windowMaterial = GetRealWindowMaterial(ConcreteSettings.WindowMaterial);
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

        [Inline(InlineBehavior.Remove)]
        private static WindowMaterial GetRealWindowMaterial(WindowMaterial material)
            => material < WindowMaterial.None || material >= WindowMaterial._Last || !SequenceHelper.Contains(SystemHelper.GetAvailableMaterials(), material) ?
            SystemHelper.GetDefaultMaterial() : material;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async void InvokeUpdateWindowFps(CoreWindow window)
        {
            await Task.Delay(2000);
            IntPtr handle = window.Handle;
            if (handle == IntPtr.Zero)
                return;
            User32.PostMessageW(handle, CustomWindowMessages.ConcreteUpdateRefreshRate, 0, 0);
        }

        #region Overrides Methods
        PointF IRenderer.GetMousePosition() => PointToClient(MouseHelper.GetMousePosition());
        #endregion
    }
}
