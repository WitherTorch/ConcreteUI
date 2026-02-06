using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Layout;
using ConcreteUI.Theme;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Structures;

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
        public abstract int PageCount { get; }

        public int CurrentPage
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pageIndex;
            set
            {
                if (_pageIndex == value)
                    return;
                RenderingController? controller = GetRenderingController();
                controller?.Lock();
                OnCurrentPageChanging();
                ClearFocusElement();
                _pageIndex = value;
                isPageChanged = true;
                OnCurrentPageChanged();
                controller?.Unlock();
            }
        }
        #endregion

        #region Events
        public event EventHandler? CurrentPageChanging;
        public event EventHandler? CurrentPageChanged;
        #endregion

        #region Constuctor
        protected PagedWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(parent, passParentToUnderlyingWindow) { }
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
        protected override IEnumerable<UIElement?> GetActiveElements()
        {
            int pageIndex = _pageIndex;
            return pageIndex < 0 ? Enumerable.Empty<UIElement>() : GetActiveElements(pageIndex);
        }

        public override IEnumerable<UIElement?> GetElements() => EnumerateActiveElementsInAllPages()
            .ConcatOptimized(GetOverlayElements())
            .ConcatOptimized(GetBackgroundElements());

        protected override void RecalculatePageLayout(in RectF pageRect)
        {
            int pageIndex = _pageIndex;
            if (pageIndex < 0)
                return;
            RecalculatePageLayout(pageRect, pageIndex);
            _recalcState.InterlockedExchange(1UL << pageIndex);
        }

        protected override void RenderPage(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect, bool force)
        {
            if (RecalculateLayoutIfPageChanged(pageRect))
                force = true;
            RenderPageCore(deviceContext, collector, pageRect, force);
        }

        protected virtual void RenderPageCore(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, in RectF pageRect, bool force)
            => base.RenderPage(deviceContext, collector, pageRect, force);

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            _recalcState.InterlockedExchange(0);
        }

        #endregion

        #region Virtual Methods
        protected virtual void RecalculatePageLayout(in RectF pageRect, int pageIndex)
        {
            Rect flooredPageRect = (Rect)pageRect;
            LayoutEngine layoutEngine = RentLayoutEngine();
            layoutEngine.RecalculateLayout(flooredPageRect, GetActiveElements(pageIndex));
            layoutEngine.RecalculateLayout(flooredPageRect, GetOverlayElements());
            ReturnLayoutEngine(layoutEngine);
        }

        protected virtual IEnumerable<UIElement?> EnumerateActiveElementsInAllPages()
        {
            int pageCount = PageCount;
            for (int i = 0; i < pageCount; i++)
            {
                foreach (UIElement? element in GetActiveElements(i))
                    yield return element;
            }
        }
        #endregion

        #region Abstract Methods
        protected abstract IEnumerable<UIElement?> GetActiveElements(int pageIndex);
        #endregion

        #region Normal Methods
        protected bool RecalculateLayoutIfPageChanged(in RectF pageRect)
        {
            if (isPageChanged)
            {
                isPageChanged = false;
                int pageIndex = _pageIndex;
                if (pageIndex < 0)
                    return true;
                if (!_recalcState.InterlockedSet(pageIndex, true))
                    RecalculatePageLayout(pageRect, pageIndex);
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
            _recalcState.InterlockedSet(pageIndex, false);
            isPageChanged = true;
            Update();
        }
        #endregion
    }
}
