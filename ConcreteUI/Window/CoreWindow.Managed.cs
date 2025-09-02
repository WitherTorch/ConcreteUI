using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Native;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

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
        private uint _dpi = 96;
        private float _dpiScaleFactor = 1.0f; // 螢幕DPI / 96
        private float _windowScaleFactor = 1.0f; //  96 / 螢幕DPI
        private BitVector64 titleBarStates = ulong.MaxValue;
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

        protected virtual void OnDpiChanged()
        {
            DpiChanged?.Invoke(this, EventArgs.Empty);
            OnDpiChangedRenderingPart();
        }

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
        public uint Dpi => _dpi;
        public float DpiScaleFactor => _dpiScaleFactor;
        public float WindowScaleFactor => _windowScaleFactor;

        public new RectangleF Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (RectangleF)ScalingRectF((Rect)base.Bounds, _windowScaleFactor);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => base.Bounds = (Rectangle)ScalingRect((RectF)value, _dpiScaleFactor);
        }

        public new RectangleF ClientBounds => (RectangleF)ScalingRectF((Rect)base.ClientBounds, _windowScaleFactor);
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
                    bool state = titleBarStates[2];
                    if (state == value)
                        return;
                    titleBarStates[2] = value;

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
                    bool state = titleBarStates[1];
                    if (state == value)
                        return;
                    titleBarStates[1] = value;

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
                    bool state = titleBarStates[0];
                    if (state == value)
                        return;
                    titleBarStates[0] = value;

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

        #region Overrides Methods
        PointF IRenderer.GetMousePosition() => PointToClient(MouseHelper.GetMousePosition());
        #endregion
    }
}
