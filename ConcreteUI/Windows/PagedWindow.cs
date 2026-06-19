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

namespace ConcreteUI.Windows;

public abstract class PagedWindow : CoreWindow
{
    #region Fields
    private BitVector64 _recalcState;
    private uint _pageIndex;
    private bool _isPageChanged;
    #endregion

    #region Properties
    protected bool IsPageChanged => _isPageChanged;

    public abstract uint PageCount { get; }

    public uint CurrentPage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pageIndex;
        set
        {
            if (_pageIndex == value)
                return;
            RenderingController? controller = GetRenderingController();
            controller?.Lock();
            try
            {
                OnCurrentPageChanging();
                ClearFocusElement();
                _pageIndex = value;
                _isPageChanged = true;
                OnCurrentPageChanged();
            }
            finally
            {
                if (controller is not null)
                {
                    controller.RequestUpdate(force: true);
                    controller.Unlock();
                }
            }
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
        uint pageIndex = _pageIndex;
        return GetActiveElements(pageIndex);
    }

    public override IEnumerable<UIElement?> GetElements() => EnumerateActiveElementsInAllPages()
        .ConcatOptimized(GetOverlayElement());

    protected override void RecalculatePageLayout(Size pageSize)
    {
        uint pageIndex = _pageIndex;
        RecalculatePageLayout(pageSize, pageIndex);
        _recalcState.InterlockedExchange(1UL << (int)pageIndex);
    }

    protected override void RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
    {
        if (RecalculatePageLayoutIfPageChanged(data.PageBounds.Size))
            context.UsePresentAllModeOnce();
        RenderPageCore(context, in data);
    }

    protected virtual void RenderPageCore(in RegionalRenderingContext context, in WindowRenderingData data)
        => base.RenderPage(context, in data);

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        base.ApplyThemeCore(provider);
        _recalcState.InterlockedExchange(0);
    }

    #endregion

    #region Virtual Methods
    protected virtual void RecalculatePageLayout(Size pageSize, uint pageIndex)
    {
        using LayoutEngineRentScope engine = LayoutEngine.Rent();
        engine.RecalculateLayout(pageSize, GetActiveElements(pageIndex));
        engine.RecalculateLayout(pageSize, GetOverlayElement());
        Thread.MemoryBarrier();
    }

    protected virtual IEnumerable<UIElement?> EnumerateActiveElementsInAllPages()
    {
        uint pageCount = PageCount;
        for (uint i = 0; i < pageCount; i++)
        {
            foreach (UIElement? element in GetActiveElements(i))
                yield return element;
        }
    }
    #endregion

    #region Abstract Methods
    protected abstract IEnumerable<UIElement?> GetActiveElements(uint pageIndex);
    #endregion

    #region Normal Methods
    protected bool RecalculatePageLayoutIfPageChanged(Size pageSize)
    {
        if (_isPageChanged)
        {
            _isPageChanged = false;
            uint pageIndex = _pageIndex;
            if (!_recalcState.InterlockedSet(pageIndex, true))
            {
                RecalculatePageLayout(pageSize, pageIndex);
                Thread.MemoryBarrier();
            }
            return true;
        }
        return false;
    }

    protected void UpdateAndResize(int pageIndex)
    {
        if (pageIndex < 0)
        {
            UpdateAndResize();
            return;
        }
        _recalcState.InterlockedSet(pageIndex, false);
        _isPageChanged = true;
        Update();
    }
    #endregion
}
