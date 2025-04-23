using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using ConcreteUI.Controls;
using ConcreteUI.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Window
{
    public abstract partial class CoreWindow : Form
    {
        #region Static Fields
        private static readonly List<WeakReference<CoreWindow>> _rootWindowList = new List<WeakReference<CoreWindow>>();
        #endregion

        #region Fields
        private readonly List<WeakReference<CoreWindow>> _childrenReferenceList = new List<WeakReference<CoreWindow>>();
        private readonly CoreWindow? _parent;
        private int dpi = 96;
        private float dpiScaleFactor = 1.0f; // 螢幕DPI / 96
        private float windowScaleFactor = 1.0f; //  96 / 螢幕DPI
        private BitVector64 titleBarStates = ulong.MaxValue;
        #endregion

        #region Events
        public event EventHandler<FormWindowState>? WindowStateChanging;
        public event EventHandler? WindowStateChanged;
        public new event EventHandler? DpiChanged;
        #endregion

        #region Event Triggers
        protected virtual void OnWindowStateChanging(FormWindowState windowState)
        {
            WindowStateChanging?.Invoke(this, windowState);
            OnWindowStateChangingRenderingPart(windowState);
        }

        protected virtual void OnWindowStateChanged()
        {
            WindowStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDpiChanged()
        {
            DpiChanged?.Invoke(this, EventArgs.Empty);
            OnDpiChangedRenderingPart();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            float windowScaleFactor = this.windowScaleFactor;
            OnMouseDownForElements(new MouseInteractEventArgs(ScalingPointF(e.Location, windowScaleFactor), e.Button));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            float windowScaleFactor = this.windowScaleFactor;
            OnMouseUpForElements(new MouseInteractEventArgs(ScalingPointF(e.Location, windowScaleFactor), e.Button));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            float windowScaleFactor = this.windowScaleFactor;
            OnMouseMoveForElements(new MouseInteractEventArgs(ScalingPointF(e.Location, windowScaleFactor)));
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            float windowScaleFactor = this.windowScaleFactor;
            OnMouseScrollForElements(new MouseInteractEventArgs(ScalingPointF(e.Location, windowScaleFactor), e.Delta));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            OnKeyDownForElements(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            OnKeyUpForElements(e);
        }
        #endregion

        #region Properties
        public new CoreWindow? Parent => _parent;
        public IThemeContext? CurrentTheme => _resourceProvider?.ThemeContext;
        public int Dpi => dpi;
        public float DpiScaleFactor => dpiScaleFactor;
        public float WindowScaleFactor => windowScaleFactor;
        public new bool MaximizeBox
        {
            get => titleBarStates[2];
            set
            {
                bool state = titleBarStates[2];
                if (state != value)
                {
                    titleBarStates[2] = value;
                    base.MaximizeBox = value;
                    Update();
                }
            }
        }

        public new bool MinimizeBox
        {
            get => titleBarStates[1];
            set
            {
                bool state = titleBarStates[1];
                if (state != value)
                {
                    titleBarStates[1] = value;
                    base.MinimizeBox = value;
                    Update();
                }
            }
        }

        public bool ShowTitle
        {
            get => titleBarStates[0];
            set
            {
                bool state = titleBarStates[0];
                if (state != value)
                {
                    titleBarStates[0] = value;
                    Update();
                }
            }
        }
        #endregion

        protected CoreWindow(CoreWindow? parent)
        {
            _parent = parent;
            List<WeakReference<CoreWindow>> windowList = parent is null ? _rootWindowList : parent._childrenReferenceList;
            lock (windowList)
                windowList.Add(new WeakReference<CoreWindow>(this));
            _windowMaterial = parent is null ? GetRealWindowMaterial(ConcreteSettings.WindowMaterial) : parent.WindowMaterial;
            AutoScaleDimensions = SizeF.Empty;
            AutoScaleMode = AutoScaleMode.None;
            InitUnmanagedPart();
        }

        [Inline(InlineBehavior.Remove)]
        private static WindowMaterial GetRealWindowMaterial(WindowMaterial material)
            => material < WindowMaterial.None || material >= WindowMaterial._Last || !SequenceHelper.Contains(SystemHelper.GetAvailableMaterials(), material) ?
            SystemHelper.GetDefaultMaterial() : material;

        #region Overrides Methods
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (specified != BoundsSpecified.All)
            {
                base.SetBoundsCore(x, y, width, height, specified);
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            TriggerResize();
        }

        PointF IRenderer.GetMousePosition()
        {
            return ScalingPointF(MousePosition, WindowScaleFactor);
        }
        #endregion

        #region Inline Macros
        [Inline(InlineBehavior.Remove)]
        private static MouseEventArgs Scale(MouseEventArgs e, float windowScaleFactor)
        {
            if (windowScaleFactor == 1f) return e;
            else return new MouseEventArgs(e.Button, e.Clicks, MathI.Floor(e.X * windowScaleFactor), MathI.Floor(e.Y * windowScaleFactor), e.Delta);
        }
        #endregion
    }
}
