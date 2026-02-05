using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using ConcreteUI.Controls;
using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Window
{
    public abstract partial class CoreWindow : NativeWindow
    {
        #region Static Fields
        private static readonly List<WeakReference<CoreWindow>> _rootWindowList = new List<WeakReference<CoreWindow>>();
        #endregion

        #region Fields
        private readonly List<WeakReference<CoreWindow>> _childrenReferenceList = new List<WeakReference<CoreWindow>>();
        private readonly CoreWindow? _parent;
        private PointU _dpi = SystemConstants.DefaultDpi;
        private Vector2 _pointsPerPixel = Vector2.One; // 螢幕DPI / 96
        private Vector2 _pixelsPerPoint = Vector2.One; //  96 / 螢幕DPI
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

        protected virtual void OnMouseDown(ref MouseInteractEventArgs args)
            => OnMouseDownForElements(ref args);

        protected virtual void OnMouseUp(in MouseNotifyEventArgs args)
            => OnMouseUpForElements(in args);

        protected virtual void OnMouseMove(in MouseNotifyEventArgs args)
            => OnMouseMoveForElements(in args);

        protected virtual void OnMouseScroll(ref MouseInteractEventArgs args)
            => OnMouseScrollForElements(ref args);

        protected virtual void OnKeyDown(ref KeyInteractEventArgs args)
            => OnKeyDownForElements(ref args);

        protected virtual void OnKeyUp(ref KeyInteractEventArgs args)
            => OnKeyUpForElements(ref args);
        #endregion

        #region Properties
        public CoreWindow? Parent => _parent;
        public IThemeContext? CurrentTheme => _resourceProvider?.ThemeContext;
        public PointU Dpi => _dpi;
        public Vector2 DpiScaleFactor => _pointsPerPixel;
        public Vector2 WindowScaleFactor => _pixelsPerPoint;

        public new RectangleF Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (RectangleF)ScalingPixelToLogical((Rect)base.Bounds, _pixelsPerPoint);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => base.Bounds = (Rectangle)ScalingLogicalToPixel((RectF)value, _pointsPerPixel);
        }

        public new RectangleF ClientBounds => (RectangleF)ScalingPixelToLogical((Rect)base.ClientBounds, _pixelsPerPoint);
        public new PointF Location => Bounds.Location;
        public new SizeF Size => Bounds.Size;
        public new SizeF ClientSize => ClientBounds.Size;
        public new float X => Bounds.X;
        public new float Y => Bounds.Y;
        public new float Width => Bounds.Width;
        public new float Height => Bounds.Height;

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
        protected CoreWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(passParentToUnderlyingWindow ? parent : null)
        {
            _parent = parent;
            List<WeakReference<CoreWindow>> windowList = parent is null ? _rootWindowList : parent._childrenReferenceList;
            lock (windowList)
                windowList.Add(new WeakReference<CoreWindow>(this));
            _windowMaterial = parent is null ? GetRealWindowMaterial(ConcreteSettings.WindowMaterial) : parent.WindowMaterial;
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
