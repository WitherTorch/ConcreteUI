using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Controls;
using ConcreteUI.Graphics;
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
        protected bool _isPageChanged;
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
                _isPageChanged = true;
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
        protected PagedWindow() : base() { }

        protected PagedWindow(GraphicsDeviceProvider? deviceProvider) : base(deviceProvider) { }

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

        protected override void RecalculatePageLayout(Size pageSize)
        {
            int pageIndex = _pageIndex;
            if (pageIndex < 0)
                return;
            RecalculatePageLayout(pageSize, pageIndex);
            _recalcState.InterlockedExchange(1UL << pageIndex);
        }

        protected override void RenderPage(in RegionalRenderingContext context)
        {
            if (RecalculatePageLayoutIfPageChanged(_pageRect.Size))
                context.UsePresentAllModeOnce();
            RenderPageCore(context);
        }

        protected virtual void RenderPageCore(in RegionalRenderingContext context)
            => base.RenderPage(context);

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            _recalcState.InterlockedExchange(0);
        }

        #endregion

        #region Virtual Methods
        protected virtual void RecalculatePageLayout(Size pageSize, int pageIndex)
        {
            LayoutEngine layoutEngine = RentLayoutEngine();
            try
            {
                layoutEngine.RecalculateLayout(pageSize, GetActiveElements(pageIndex));
                layoutEngine.RecalculateLayout(pageSize, GetOverlayElements());
            }
            finally
            {
                ReturnLayoutEngine(layoutEngine);
            }
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
        protected bool RecalculatePageLayoutIfPageChanged(Size pageSize)
        {
            if (_isPageChanged)
            {
                _isPageChanged = false;
                int pageIndex = _pageIndex;
                if (pageIndex < 0)
                    return true;
                if (!_recalcState.InterlockedSet(pageIndex, true))
                {
                    RecalculatePageLayout(pageSize, pageIndex);
                    Thread.MemoryBarrier();
                }
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
            _isPageChanged = true;
            Update();
        }
        #endregion
    }
}
