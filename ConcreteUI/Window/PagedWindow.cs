using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Layout;
using ConcreteUI.Theme;

using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window
{
    public abstract class PagedWindow : CoreWindow
    {
        #region Fields
        private int _pageIndex;
        private BitVector64 _recalcState;
        protected bool isPageChanged;
        #endregion

        #region Properties
        public int CurrentPage
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pageIndex;
            set
            {
                if (_pageIndex == value)
                    return;
                OnCurrentPageChanging();
                ClearFocusElement();
                _pageIndex = value;
                isPageChanged = true;
                OnCurrentPageChanged();
                Update();
            }
        }
        #endregion

        #region Events
        public event EventHandler? CurrentPageChanging;
        public event EventHandler? CurrentPageChanged;
        #endregion

        #region Constuctor
        protected PagedWindow(CoreWindow? parent) : base(parent) { }
        #endregion

        #region Event Triggers
        protected virtual void OnCurrentPageChanging()
        {
            CurrentPageChanging?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCurrentPageChanged()
        {
            CurrentPageChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Override Methods
        protected override IEnumerable<UIElement> GetRenderingElements() => GetRenderingElements(_pageIndex);

        protected override void RecalculatePageLayout(in Rect pageRect)
        {
            int pageIndex = _pageIndex;
            _recalcState.InterlockedExchange(1UL << pageIndex);
            RecalculatePageLayout(pageRect, pageIndex);
        }

        protected override void RenderPage(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect, bool force)
        {
            if (RecalculateLayoutIfPageChanged(pageRect))
                force = true;
            base.RenderPage(deviceContext, collector, pageRect, force);
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            _recalcState.InterlockedExchange(ulong.MaxValue);
        }

        #endregion

        #region Virtual Methods
        protected virtual void RecalculatePageLayout(in Rect pageRect, int pageIndex)
        {
            LayoutEngine layoutEngine = RentLayoutEngine();
            layoutEngine.RecalculateLayout(pageRect, GetRenderingElements(pageIndex));
            layoutEngine.RecalculateLayout(pageRect, GetOverlayElements());
            ReturnLayoutEngine(layoutEngine);
        }
        #endregion

        #region Abstract Methods
        protected abstract IEnumerable<UIElement> GetRenderingElements(int pageIndex);
        #endregion

        #region Normal Methods
        protected bool RecalculateLayoutIfPageChanged(in RectF pageRect)
        {
            if (isPageChanged)
            {
                isPageChanged = false;
                int pageIndex = _pageIndex;
                if (!_recalcState.InterlockedSet(pageIndex, true))
                    RecalculatePageLayout((Rect)pageRect, pageIndex);
                return true;
            }
            return false;
        }

        protected void TriggerResize(int pageIndex)
        {
            if (pageIndex < 0)
            {
                TriggerResize();
                return;
            }
            if (_recalcState.InterlockedSet(pageIndex, false))
                Refresh();
        }
        #endregion
    }
}
