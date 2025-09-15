using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ComboBoxDropdownList : ScrollableElementBase, IMouseNotifyEvents
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
        private readonly ComboBox _parent;
        private readonly CoreWindow _window;

        private DWriteTextLayout[]? _layouts;
        private float _itemHeight;
        private int _selectedIndex, _maxViewCount;
        private bool _isClicking, _isClickingClient, _isFirstTimeClick;

        public ComboBoxDropdownList(ComboBox parent, CoreWindow window) : base(window, "app.comboBox")
        {
            ScrollBarType = ScrollBarType.AutoVertial;
            _parent = parent;
            _window = window;
            _isFirstTimeClick = true;
            _selectedIndex = -1;
            LeftVariable = parent.LeftReference;
            RightVariable = parent.RightReference;
            TopVariable = new DefaultTopVariable(this);
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            ComboBox parent = _parent;
            using DWriteTextFormat format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleLeft, provider.FontName, parent.FontSize);
            Prepare(format);
        }

        protected override D2D1Brush GetBackBrush() => _brushes[(int)Brush.BackBrush];

        protected override D2D1Brush GetBackDisabledBrush() => _brushes[(int)Brush.BackDisabledBrush];

        protected override D2D1Brush GetBorderBrush() => _brushes[(int)Brush.BorderBrush];

        public void Close() => _window.CloseOverlayElement(this);

        public void Prepare(DWriteTextFormat format)
        {
            ComboBox parent = _parent;
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

            float pointsPerPixel = _window.GetPointsPerPixel();
            float borderWidth = RenderingHelper.GetDefaultBorderWidth(pointsPerPixel);
            itemHeight = RenderingHelper.CeilingInPixel(itemHeight, pointsPerPixel) + borderWidth * 2;
            _itemHeight = itemHeight;

            int maxViewCount = MathHelper.Min(parent.DropdownListVisibleCount, count);
            int lastIndex = MathHelper.Clamp(parent.SelectedIndex, -1, count - 1);
            _selectedIndex = -1;
            _maxViewCount = maxViewCount;
            SurfaceSize = new Size(0, MathI.Ceiling(itemHeight * count));
            float elementHeight = maxViewCount * itemHeight;
            HeightVariable = MathI.Ceiling(elementHeight);
            if (lastIndex > maxViewCount / 2)
                ScrollingTo(MathI.Ceiling(lastIndex * itemHeight + itemHeight / 2 - elementHeight / 2));
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
            int startIndex = MathHelper.Clamp(MathI.FloorPositive(startIndexRaw), 0, count - maxViewCount);
            int endIndex = MathHelper.Clamp(MathI.Ceiling((viewportY + renderSize.Height) / itemHeight), maxViewCount - 1, count - 1);
            int selectedIndex = SelectedIndex;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush textBrush;
            float pointsPerPixel = context.PointsPerPixel;
            float borderWidth = context.DefaultBorderWidth;
            float itemLeft = borderWidth,
                textLeft = RenderingHelper.RoundInPixel(borderWidth + 5, pointsPerPixel),
                itemTop = RenderingHelper.RoundInPixel(-offsetY, pointsPerPixel),
                itemRight = RenderingHelper.RoundInPixel(renderSize.Width - borderWidth, pointsPerPixel),
                itemWIdth = itemRight - itemLeft;
            textBrush = brushes[(int)Brush.TextBrush];
            ref DWriteTextLayout layoutArrayRef = ref layouts[0];
            for (int i = startIndex; i <= endIndex; i++)
            {
                RectF itemBounds = new RectF(itemLeft, itemTop, itemRight, itemTop + itemHeight);
                using RegionalRenderingContext clipContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                if (itemBounds.IsEmpty)
                    break;

                D2D1Brush activeTextBrush;
                if (i == selectedIndex)
                {
                    D2D1Brush activeBackBrush = isClicking ? brushes[(int)Brush.ListBackPressedBrush] : brushes[(int)Brush.ListBackHoveredBrush];
                    RenderBackground(context, activeBackBrush);
                    activeTextBrush = brushes[(int)Brush.ListTextHoveredBrush];
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

        public void OnMouseDown(in MouseNotifyEventArgs args)
        {
            _isClicking = false;
            _isClickingClient = false;
        }

        public override void OnMouseDown(ref MouseInteractEventArgs args)
        {
            base.OnMouseDown(ref args);
            if (args.Handled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;
            args.Handle();
            _isClicking = true;
            _isClickingClient = ContentBounds.Contains(args.Location);
        }

        public override void OnMouseUp(in MouseNotifyEventArgs args)
        {
            base.OnMouseUp(in args);
            if (!args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
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
                    Close();
                    ItemClicked?.Invoke(this, selectedIndex);
                    return;
                }
            }
            else if (!bounds.Contains(args.X, args.Y))
            {
                Close();
                return;
            }
            Update();
        }

        public override void OnMouseMove(in MouseNotifyEventArgs args)
        {
            base.OnMouseMove(args);
            Rect bounds = ContentBounds;
            if (bounds.Contains(args.Location) && (!_isClicking || _isClickingClient))
            {
                float y = args.Y - Location.Y + ViewportPoint.Y;
                int hoverIndex = MathI.Floor(y / _itemHeight);
                if (hoverIndex < 0 || hoverIndex >= _parent.Items.Count)
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
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        private sealed class DefaultTopVariable : LayoutVariable
        {
            private readonly WeakReference<ComboBoxDropdownList> _ownerRef;

            public DefaultTopVariable(ComboBoxDropdownList owner)
            {
                _ownerRef = new WeakReference<ComboBoxDropdownList>(owner);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_ownerRef.TryGetTarget(out ComboBoxDropdownList? owner))
                    return 0;
                int val = manager.GetComputedValue(owner._parent, LayoutProperty.Bottom);
                return val - MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(owner._window.GetPointsPerPixel()));
            }
        }
    }
}
