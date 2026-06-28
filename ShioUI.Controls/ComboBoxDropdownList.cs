using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;

using ShioUI.Graphics.Helpers;
using ShioUI.Utils;
using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Theme;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using ShioUI.Extensions;

namespace ShioUI.Controls;

public sealed partial class ComboBoxDropdownList : ScrollableElementBase, IGlobalMouseInteractHandler
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "back",
        "back.disabled",
        "border",
        "fore",
        "list.back.hovered",
        "list.back.pressed",
        "list.fore.hovered",
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly ComboBox _owner;

    private DWriteTextLayout[]? _layouts;
    private float _itemHeight;
    private int _selectedIndex, _maxViewCount, _maxViewHeight;
    private bool _isClicking, _isClickingClient, _isFirstTimeClick, _prepareToClose;

    public ComboBoxDropdownList(IElementContainer parent, ComboBox owner) : base(parent, "app.comboBox")
    {
        ScrollBarType = ScrollBarType.AutoVertial;
        _owner = owner;
        _isFirstTimeClick = true;
        _selectedIndex = -1;

        Rectangle ownerBounds = owner.Bounds;
        Point ownerLocation = owner.LocalPageToGlobalPage(ownerBounds.Location);
        Size ownerSize = ownerBounds.Size;

        LeftExpression = ownerLocation.X;
        RightExpression = ownerLocation.X + ownerSize.Width;
        TopExpression = new DefaultTopNode(this, ownerLocation.Y + ownerSize.Height);
        HeightExpression = new DefaultHeightNode(this);
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        base.ApplyThemeCore(provider);
        UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        ComboBox parent = _owner;
        using DWriteTextFormat format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleLeft, provider.FontName, parent.FontSize);
        Prepare(parent, format);
    }

    protected override D2D1Brush GetBackBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush);

    protected override D2D1Brush GetBackDisabledBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackDisabledBrush);

    protected override D2D1Brush GetBorderBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BorderBrush);

    public void Close() => RootWindow.CloseOverlayElement(this);

    private void Prepare(ComboBox parent, DWriteTextFormat format)
    {
        DWriteFactory factory = SharedResources.DWriteFactory;

        float itemHeight = 0f;
        IList<string> items = parent.Items;
        int count = items.Count;
        if (count <= 0)
            return;
        DWriteTextLayout[] layouts = new DWriteTextLayout[count];
        for (int i = 0; i < count; i++)
        {
            DWriteTextLayout layout = factory.CreateTextLayout(items[i], format);
            layouts[i] = layout;
            itemHeight = MathHelper.Max(itemHeight, layout.GetMetrics().Height);
        }
        DisposeHelper.SwapDispose(ref _layouts, layouts);

        Vector2 pixelsPerPoint = Window.GetPixelsPerPoint();
        float borderWidth = RenderingHelper.GetDefaultBorderWidth(pixelsPerPoint.X);
        itemHeight = RenderingHelper.CeilingInPixel(itemHeight, pixelsPerPoint.Y) + borderWidth * 2;
        _itemHeight = itemHeight;

        int maxViewCount = MathHelper.Min(parent.DropdownListVisibleCount, count);
        int lastIndex = MathHelper.Clamp(parent.SelectedIndex, -1, count - 1);
        _selectedIndex = -1;
        _maxViewCount = maxViewCount;
        SurfaceSize = new Size(0, MathI.Ceiling(itemHeight * count));

        float maxViewHeight = maxViewCount * itemHeight;
        InterlockedHelper.Write(ref _maxViewHeight, MathI.Ceiling(maxViewHeight));
        if (lastIndex > maxViewCount / 2)
            ScrollingTo(MathI.Ceiling(lastIndex * itemHeight + itemHeight / 2 - maxViewHeight / 2));
    }

    protected override bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
    {
        if (context.HasDirtyCollector)
        {
            RenderBackground(context, backBrush);
            context.MarkAsDirty();
        }
        DWriteTextLayout[]? layouts = Interlocked.Exchange(ref _layouts, null);
        if (layouts is null)
            return true;
        SizeF renderSize = context.Size;
        int count = layouts.Length;
        if (count <= 0 || renderSize.IsEmpty)
            return true;
        bool isClicking = _isClicking;
        float itemHeight = _itemHeight;
        int maxViewCount = _maxViewCount;
        int viewportY = ViewportPoint.Y;
        float startIndexRaw = viewportY / itemHeight;
        float offsetY = (startIndexRaw - MathF.Floor(startIndexRaw)) * itemHeight;
        int startIndex = MathHelper.Clamp(MathI.Truncate(startIndexRaw), 0, count - maxViewCount);
        int endIndex = MathHelper.Clamp(MathI.Ceiling((viewportY + renderSize.Height) / itemHeight), maxViewCount - 1, count - 1);
        int selectedIndex = SelectedIndex;
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        D2D1Brush textBrush;
        Vector2 pointsPerPixel = context.PixelsPerPoint;
        float borderWidth = context.DefaultBorderWidth;
        float itemLeft = borderWidth,
            textLeft = RenderingHelper.RoundInPixel(borderWidth + 5, pointsPerPixel.X),
            itemTop = RenderingHelper.RoundInPixel(-offsetY, pointsPerPixel.Y),
            itemRight = RenderingHelper.RoundInPixel(renderSize.Width - borderWidth, pointsPerPixel.X),
            itemWIdth = itemRight - itemLeft;
        textBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextBrush);
        ref DWriteTextLayout layoutArrayRef = ref UnsafeHelper.GetArrayDataReference(layouts);
        for (int i = startIndex; i <= endIndex; i++)
        {
            RectF itemBounds = new RectF(itemLeft, itemTop, itemRight, itemTop + itemHeight);
            using RegionalRenderingContext clipContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
            if (itemBounds.IsEmpty)
                break;

            D2D1Brush activeTextBrush;
            if (i == selectedIndex)
            {
                D2D1Brush activeBackBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.ListBackHoveredBrush + MathHelper.BooleanToNativeUnsigned(isClicking));
                RenderBackground(context, activeBackBrush);
                activeTextBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.ListTextHoveredBrush);
            }
            else
                activeTextBrush = textBrush;
            DWriteTextLayout layout = UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i);
            layout.MaxWidth = itemWIdth;
            layout.MaxHeight = itemHeight;
            clipContext.DrawTextLayout(new PointF(textLeft, 0), layout, activeTextBrush, D2D1DrawTextOptions.None);
            itemTop = itemBounds.Bottom;
        }
        DisposeHelper.NullSwapOrDispose(ref _layouts, layouts);
        return true;
    }

    void IGlobalMouseInteractHandler.OnMouseDownGlobally(in MouseEventArgs args)
    {
        _isClicking = false;
        _isClickingClient = false;
    }

    void IGlobalMouseInteractHandler.OnMouseUpGlobally(in MouseEventArgs args)
    {
        if (!args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;
        _isClickingClient = false;
        if (_isFirstTimeClick)
        {
            _isFirstTimeClick = false;
            _isClicking = false;
            return;
        }
        Rect bounds = Bounds;
        if (_isClicking)
        {
            _isClicking = false;
            int selectedIndex = _selectedIndex;
            if (selectedIndex >= 0)
            {
                _prepareToClose = true;
                ItemClicked?.Invoke(this, selectedIndex);
                Close();
                return;
            }
        }
        else if (!bounds.Contains(args.X, args.Y))
        {
            _prepareToClose = true;
            Close();
            return;
        }
        Update();
    }

    protected override void OnMouseDown(ref HandleableMouseEventArgs args)
    {
        base.OnMouseDown(ref args);
        if (args.Handled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;
        args.Handle();
        _isClicking = true;
        _isClickingClient = args.IsInSpecificSize(ContentSize);
    }

    protected override void OnMouseMove(in MouseEventArgs args)
    {
        base.OnMouseMove(args);
        if (args.IsInSpecificSize(ContentSize) && (!_isClicking || _isClickingClient))
        {
            float y = args.Y + ViewportPoint.Y;
            int hoverIndex = MathI.Floor(y / _itemHeight);
            if (hoverIndex < 0 || hoverIndex >= _owner.Items.Count)
                _selectedIndex = -1;
            else
                _selectedIndex = hoverIndex;
        }
        else
        {
            _selectedIndex = -1;
        }
        Update(ScrollableElementUpdateFlags.Content);
    }

    public override void Scrolling(int scrollStep) => base.Scrolling(scrollStep / 4);

    protected override void OnScrollBarUpButtonClicked() => base.Scrolling(-MathI.Ceiling(_itemHeight));

    protected override void OnScrollBarDownButtonClicked() => base.Scrolling(MathI.Ceiling(_itemHeight));

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeInterlocked(ref _layouts);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
