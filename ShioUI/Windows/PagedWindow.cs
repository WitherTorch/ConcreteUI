using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using ShioUI.Graphics;
using ShioUI.Layout;

using RiceTea.Core.Extensions;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract class PagedWindow : CoreWindow
{
    #region Fields
    private uint _pageIndex;
    #endregion

    #region Properties
    public abstract uint PageCount { get; }

    public uint CurrentPage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pageIndex;
        set
        {
            if (_pageIndex == value)
                return;
            using BatchUpdateScope scope = EnterBatchUpdateScope();
            OnCurrentPageChanging();
            ClearFocusElement();
            _pageIndex = value;
            OnCurrentPageChanged();
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

    protected override void RecalculatePageLayout(Size pageSize, in RecalculateLayoutInformation information)
        => RecalculatePageLayout(pageSize, _pageIndex, information);

    #endregion

    #region Virtual Methods
    protected virtual void RecalculatePageLayout(Size pageSize, uint pageIndex, in RecalculateLayoutInformation information)
    {
        using LayoutEngineRentScope engine = LayoutEngine.Rent();
        engine.RecalculateLayout(pageSize, GetActiveElements(pageIndex), information);
        engine.RecalculateLayout(pageSize, GetOverlayElement(), information);
        Thread.MemoryBarrier();
    }
    #endregion

    #region Abstract Methods
    protected abstract IEnumerable<UIElement?> GetActiveElements(uint pageIndex);
    #endregion
}
