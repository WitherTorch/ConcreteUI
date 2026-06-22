using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Layout;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Windows;

public abstract class PagedWindow : CoreWindow
{
    #region Fields
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

    public override IEnumerable<UIElement?> GetElements()
    {
        uint pageCount = PageCount;
        if (pageCount <= 0)
            return Enumerable.Empty<UIElement?>();
        IEnumerable<UIElement?> elements = GetActiveElements(0);
        for (uint i = 1; i < pageCount; i++)
            elements = elements.ConcatOptimized(GetActiveElements(i));
        return elements;
    }

    protected override void RecalculatePageLayout(Size pageSize, ulong timestamp) 
        => RecalculatePageLayout(pageSize, _pageIndex, timestamp);

    #endregion

    #region Virtual Methods
    protected virtual void RecalculatePageLayout(Size pageSize, uint pageIndex, ulong timestamp)
    {
        using LayoutEngineRentScope engine = LayoutEngine.Rent();
        engine.RecalculateLayout(pageSize, GetActiveElements(pageIndex), timestamp);
        engine.RecalculateLayout(pageSize, GetOverlayElement(), timestamp);
        Thread.MemoryBarrier();
    }
    #endregion

    #region Abstract Methods
    protected abstract IEnumerable<UIElement?> GetActiveElements(uint pageIndex);
    #endregion
}
