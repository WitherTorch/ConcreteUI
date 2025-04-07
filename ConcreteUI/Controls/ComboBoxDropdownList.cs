using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ComboBoxDropdownList : ScrollableElementBase, IGlobalMouseEvents
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
        }.WithPrefix("app.comboBox.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly ComboBox _parent;
        private readonly CoreWindow _window;

        private DWriteTextLayout[] _layouts;
        private int _itemHeight, _selectedIndex, _maxViewCount;

        public ComboBoxDropdownList(ComboBox parent, CoreWindow window) : base(window)
        {
            ScrollBarType = ScrollBarType.AutoVertial;
            _parent = parent;
            _window = window;
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            base.ApplyTheme(provider);
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
        }

        protected override D2D1Brush GetBackBrush() => _brushes[(int)Brush.BackBrush];

        protected override D2D1Brush GetBackDisabledBrush() => _brushes[(int)Brush.BackDisabledBrush];

        protected override D2D1Brush GetBorderBrush() => _brushes[(int)Brush.BorderBrush];

        public void Close() => _window.CloseOverlayElement(this);

        public void Prepare(DWriteTextFormat format)
        {
            if (InterlockedHelper.Read(ref _layouts) is not null) // Already shown
                return;

            ComboBox parent = _parent;
            DWriteFactory factory = SharedResources.DWriteFactory;

            float maxHeight = 0f;
            IList<string> items = parent.Items;
            int count = items.Count;
            if (count <= 0)
                return;
            Rectangle parentBounds = parent.Bounds;
            float lineWidth = Renderer.GetBaseLineWidth();
            float itemWidth = parentBounds.Width - lineWidth * 2f - 18;
            DWriteTextLayout[] layouts = new DWriteTextLayout[count];
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = factory.CreateTextLayout(items[i], format, itemWidth, float.PositiveInfinity);
                layouts[i] = layout;
                float height = layout.GetMetrics().Height;
                if (maxHeight < height)
                    maxHeight = height;
            }
            Interlocked.CompareExchange(ref _layouts, layouts, null);

            int itemHeight = MathI.Ceiling(maxHeight) + 2;
            _itemHeight = itemHeight;

            int maxViewCount = parent.DropdownListVisibleCount;
            Bounds = new Rectangle(parentBounds.Left, parentBounds.Bottom, parentBounds.Width, maxViewCount * itemHeight);
            int selectedIndex = MathHelper.Clamp(_selectedIndex, -1, count - 1);
            _selectedIndex = selectedIndex;
            _maxViewCount = maxViewCount;
            SurfaceSize = new Point(0, itemHeight * count);
        }

        protected override bool RenderContent(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            DWriteTextLayout[] layouts = _layouts;
            float lineWidth = Renderer.GetBaseLineWidth();
            Rect bounds = ContentBounds;
            int count = layouts.Length;
            if (count <= 0)
                return true;
            bool isClicking = _isClicking;
            int maxViewCount = _maxViewCount;
            int itemHeight = _itemHeight;
            int viewportY = ViewportPoint.Y;
            int startIndex = MathHelper.Clamp(Math.DivRem(viewportY, itemHeight, out int offsetY), 0, count - maxViewCount);
            int endIndex = MathHelper.Clamp(MathHelper.CeilDiv(viewportY + bounds.Height, itemHeight), maxViewCount - 1, count - 1);
            int selectedIndex = SelectedIndex;
            int x = bounds.X;
            int y = bounds.Y - offsetY;
            int right = bounds.Right;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush textBrush; 
            if (selectedIndex >= startIndex && selectedIndex <= endIndex)
            {
                RectF itemBounds = RectF.FromXYWH(x, y + (selectedIndex - startIndex) * itemHeight, right - x, itemHeight);
                context.PushAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                D2D1Brush backBrush = isClicking ? brushes[(int)Brush.ListBackPressedBrush] : brushes[(int)Brush.ListBackHoveredBrush];
                textBrush = brushes[(int)Brush.ListTextHoveredBrush];
                RenderBackground(context, backBrush);
                context.DrawTextLayout(new PointF(x + 5 + lineWidth, y), layouts[selectedIndex], textBrush);
                context.PopAxisAlignedClip();
            }
            textBrush = brushes[(int)Brush.TextBrush];
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i == selectedIndex)
                    continue;
                DWriteTextLayout layout = layouts[i];
                RectF itemBounds = new RectF(x, y, right, y + itemHeight);
                context.PushAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                context.DrawTextLayout(new PointF(x + 5 + lineWidth, y), layout, textBrush, D2D1DrawTextOptions.Clip);
                context.PopAxisAlignedClip();
                y += itemHeight;
            }
            collector.MarkAsDirty(bounds);
            return true;
        }

        bool _isClicking, _isFirstTimeClick = true;

        public override void OnMouseDown(in MouseInteractEventArgs args)
        {
            base.OnMouseDown(args);
            if (Bounds.Contains(args.X, args.Y))
            {
                _isClicking = true;
            }
        }

        public override void OnMouseUp(in MouseInteractEventArgs args)
        {
            base.OnMouseUp(args);
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
                if (_selectedIndex != -1)
                    ItemClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (!bounds.Contains(args.X, args.Y))
            {

            }
            Update();
        }

        public override void OnMouseMove(in MouseInteractEventArgs args)
        {
            base.OnMouseMove(args);
            Rect bounds = ContentBounds;
            if (bounds.Contains(args.Location))
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
            Update();
        }

        protected override void OnScrollBarUpButtonClicked() => base.Scrolling(-_itemHeight, true);

        protected override void OnScrollBarDownButtonClicked() => base.Scrolling(_itemHeight, true);

        protected override void Scrolling(int scrollStep, bool update) => base.Scrolling(scrollStep / 4, update);

        protected override void DisposeCore(bool disposing)
        {
            DWriteTextLayout[] layouts = _layouts;
            for (int i = 0, count = layouts.Length; i < count; i++)
            {
                layouts[i].Dispose();
            }
        }
    }
}
