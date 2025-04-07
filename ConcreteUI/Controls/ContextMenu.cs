using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ContextMenu : PopupElementBase, IDisposable, IKeyEvents
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "back.hovered",
            "back.pressed",
            "border",
            "fore",
            "fore.inactive",
            "fore.hovered"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
      
        private DWriteTextLayout[] _layouts;
        private float _itemHeight;
        private int _hoveredIndex;
        private bool _isPressed, _disposed;

        public ContextMenu(CoreWindow window, ContextMenuItem[] items) : base(window)
        {
            MenuItems = items;
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            ContextMenuItem[] items = MenuItems;
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            int count = items.Length;
            float itemHeight = 0f, itemWidth = 50f;
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat format = factory.CreateTextFormat(provider.FontName, 14);
            format.ParagraphAlignment = DWriteParagraphAlignment.Center;
            DWriteTextLayout[] layouts = new DWriteTextLayout[count];
            float lineWidth = Renderer.GetBaseLineWidth();
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = factory.CreateTextLayout(items[i].Text, format);
                DWriteTextMetrics metrics = layout.GetMetrics();
                float width = metrics.Width;
                float height = metrics.Height + lineWidth * 2;
                if (itemHeight < height)
                    itemHeight = height;
                if (itemWidth < width)
                    itemWidth = width;
                layouts[i] = layout;
            }
            itemHeight = MathF.Ceiling(itemHeight);
            for (int i = 0; i < count; i++)
            {
                DWriteTextLayout layout = layouts[i];
                layout.MaxHeight = itemHeight;
                layout.MaxWidth = itemWidth;
            }
            format.Dispose();
            Size size = new Size(MathI.Ceiling(itemWidth + lineWidth * 2) + 12, (int)itemHeight * count);
            _itemHeight = itemHeight;
            Size = size;
            DisposeHelper.SwapDisposeInterlocked(ref _layouts, layouts);
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            float lineWidth = Renderer.GetBaseLineWidth();
            RectF bounds = GraphicsUtils.AdjustRectangleF(Bounds);
            RectF borderBounds = GraphicsUtils.AdjustRectangleFAsBorderBounds(bounds, lineWidth);
            RectF itemBounds = RectF.FromXYWH(borderBounds.X, borderBounds.Y - lineWidth * 0.5f, borderBounds.Width, _itemHeight);
            context.PushAxisAlignedClip(bounds, D2D1AntialiasMode.Aliased);
            D2D1Brush[] brushes = _brushes;
            D2D1Brush backBrush = brushes[(int)Brush.BackBrush];
            RenderBackground(context, backBrush);
            DWriteTextLayout[] layouts = Interlocked.Exchange(ref _layouts, null);
            if (layouts is null)
                return true;
            ContextMenuItem[] items = MenuItems;
            D2D1Brush foreBrush = brushes[(int)Brush.TextBrush], foreDisabledBrush = brushes[(int)Brush.TextInactiveBrush];
            int hoveredIndex = _hoveredIndex;
            if (hoveredIndex >= 0 && hoveredIndex < layouts.Length)
            {
                RectF currentItemBounds = RectF.FromXYWH(itemBounds.X, itemBounds.Y + itemBounds.Height * hoveredIndex, itemBounds.Width, itemBounds.Height);
                context.PushAxisAlignedClip(currentItemBounds, D2D1AntialiasMode.Aliased);
                D2D1Brush currentForeBrush = foreDisabledBrush;
                if (items[hoveredIndex].Enabled)
                {
                    D2D1Brush currentBackBrush = _isPressed ? brushes[(int)Brush.BackPressedBrush] : brushes[(int)Brush.BackHoveredBrush];
                    currentForeBrush = foreBrush;
                    RenderBackground(context, currentBackBrush);
                }
                PointF textLocation = currentItemBounds.Location;
                textLocation.X += 6 + lineWidth;
                context.DrawTextLayout(textLocation, layouts[hoveredIndex], currentForeBrush);
                context.PopAxisAlignedClip();
            }
            for (int i = 0, count = layouts.Length; i < count; i++)
            {
                if (i == hoveredIndex)
                    continue;
                DWriteTextLayout layout = layouts[i];
                context.PushAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                PointF textLocation = itemBounds.Location;
                textLocation.X += 6 + lineWidth;
                context.DrawTextLayout(textLocation, layout, items[i].Enabled ? foreBrush : foreDisabledBrush, D2D1DrawTextOptions.Clip);
                context.PopAxisAlignedClip();
                itemBounds.Bottom = (itemBounds.Top = itemBounds.Bottom) + _itemHeight;
            }
            context.DrawRectangle(borderBounds, brushes[(int)Brush.BorderBrush], lineWidth);
            context.PopAxisAlignedClip();
            DisposeHelper.NullSwapOrDispose(ref _layouts, layouts);
            return true;
        }

        public override void OnMouseDown(in MouseInteractEventArgs args)
        {
            base.OnMouseDown(args);
            RectangleF bounds = Bounds;
            if (bounds.Contains(args.Location))
            {
                _isPressed = true;
                Update();
                return;
            }
            Close();
        }

        public override void OnMouseUp(in MouseInteractEventArgs args)
        {
            base.OnMouseUp(args);
            if (Bounds.Contains(args.Location))
            {
                _isPressed = false;
                int hoveredIndex = _hoveredIndex;
                ContextMenuItem[] items = MenuItems;
                if (items.Length > hoveredIndex && hoveredIndex != -1)
                {
                    ContextMenuItem item = items[hoveredIndex];
                    ItemClicked?.Invoke(this, EventArgs.Empty);
                    Close();
                    item.OnClick();
                }
                return;
            }
        }

        public override void OnMouseMove(in MouseInteractEventArgs args)
        {
            base.OnMouseMove(args);
            RectangleF bound = Bounds;
            if (bound.Contains(args.Location))
            {
                float Y = args.Y - Location.Y;
                float itemHeight = _itemHeight;
                int hoveredIndex = (int)(Y / itemHeight);
                if (MenuItems.Length > hoveredIndex)
                {
                    if (_hoveredIndex != hoveredIndex)
                    {
                        _hoveredIndex = hoveredIndex;
                        Update();
                    }
                }
            }
            else
            {
                if (_hoveredIndex != -1)
                {
                    _hoveredIndex = -1;
                    Update();
                }
            }
        }

        public void OnKeyDown(KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Escape && args.Modifiers == Keys.None)
                Close();
        }

        public void OnKeyUp(KeyEventArgs args)
        {
            //Do nothing
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            DWriteTextLayout[] layouts = _layouts;
            if (disposing)
            {
                foreach (DWriteTextLayout layout in layouts)
                    layout.Dispose();
            }
            Array.Clear(layouts, 0, layouts.Length);
        }

        ~ContextMenu()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
