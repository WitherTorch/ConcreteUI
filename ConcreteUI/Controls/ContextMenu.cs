using System;
using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
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

        private DWriteTextLayout[]? _layouts;
        private float _itemHeight;
        private int _hoveredIndex;
        private bool _isPressed, _disposed;

        public ContextMenu(CoreWindow window, ContextMenuItem[] items) : base(window, "app.contextMenu")
        {
            MenuItems = items;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            DWriteFactory factory = SharedResources.DWriteFactory;

            ContextMenuItem[] items = MenuItems;
            float itemHeight = 0f, itemWidth = 0f;
            float pointsPerPixel = Renderer.GetPointsPerPixel();
            int count = items.Length;
            DWriteTextLayout[] layouts = new DWriteTextLayout[count];
            using (DWriteTextFormat format = factory.CreateTextFormat(provider.FontName, UIConstants.BoxFontSize))
            {
                format.ParagraphAlignment = DWriteParagraphAlignment.Center;
                ref ContextMenuItem itemArrayRef = ref items[0];
                ref DWriteTextLayout layoutArrayRef = ref layouts[0];
                for (int i = 0; i < count; i++)
                {
                    string text = UnsafeHelper.AddTypedOffset(ref itemArrayRef, i).Text;
                    DWriteTextLayout layout = factory.CreateTextLayout(items[i].Text, format);
                    DWriteTextMetrics metrics = layout.GetMetrics();
                    itemWidth = MathHelper.Max(itemWidth, metrics.Width);
                    itemHeight = MathHelper.Max(itemHeight, metrics.Height);
                    UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i) = layout;
                }
                itemWidth = RenderingHelper.CeilingInPixel(itemWidth, pointsPerPixel);
                itemHeight = RenderingHelper.CeilingInPixel(itemHeight + UIConstants.ElementMarginHalf, pointsPerPixel);
                for (int i = 0; i < count; i++)
                {
                    DWriteTextLayout layout = UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i);
                    layout.MaxWidth = itemWidth;
                    layout.MaxHeight = itemHeight;
                }
            }
            float borderWidth = RenderingHelper.GetDefaultBorderWidth(pointsPerPixel);
            Size size = new Size(
                width: MathI.Ceiling(itemWidth + UIConstants.ElementMargin + borderWidth * 2),
                height: MathI.Ceiling(itemHeight * count + borderWidth * 2));
            _itemHeight = itemHeight + borderWidth;
            Size = size;
            DisposeHelper.SwapDisposeInterlocked(ref _layouts, layouts);
        }

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            SizeF renderSize = context.Size;
            float borderWidth = context.DefaultBorderWidth;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush backBrush = brushes[(int)Brush.BackBrush], borderBrush = brushes[(int)Brush.BorderBrush];
            RenderBackground(context, backBrush);
            DWriteTextLayout[]? layouts = Interlocked.Exchange(ref _layouts, null);
            if (layouts is not null && layouts.Length > 0)
            {
                ref ContextMenuItem itemArrayRef = ref MenuItems[0];
                ref DWriteTextLayout layoutArrayRef = ref layouts[0];
                D2D1Brush foreBrush = brushes[(int)Brush.TextBrush], foreDisabledBrush = brushes[(int)Brush.TextInactiveBrush];
                int hoveredIndex = _hoveredIndex;
                float itemLeft = borderWidth,
                    textLeft = itemLeft + UIConstants.ElementMarginHalf,
                    itemTop = borderWidth, 
                    itemRight = renderSize.Width - borderWidth;
                for (int i = 0, count = layouts.Length; i < count; i++)
                {
                    DWriteTextLayout layout = UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i);
                    bool isEnabled = UnsafeHelper.AddTypedOffset(ref itemArrayRef, i).Enabled;
                    float itemHeight = layout.MaxHeight;
                    D2D1Brush currentForeBrush;
                    if (isEnabled)
                    {
                        if (i == hoveredIndex && isEnabled)
                        {
                            using RenderingClipToken token = context.PushAxisAlignedClip(
                                new RectF(itemLeft, itemTop, itemRight, itemTop + itemHeight), D2D1AntialiasMode.Aliased);
                            D2D1Brush currentBackBrush = _isPressed ? brushes[(int)Brush.BackPressedBrush] : brushes[(int)Brush.BackHoveredBrush];
                            currentForeBrush = brushes[(int)Brush.TextHoveredBrush];
                            RenderBackground(context, currentBackBrush);
                        }
                        else
                            currentForeBrush = foreBrush;
                    }
                    else
                        currentForeBrush = foreDisabledBrush;
                    context.DrawTextLayout(new PointF(textLeft, itemTop), layout, currentForeBrush, D2D1DrawTextOptions.NoSnap);
                    itemTop += itemHeight;
                }
                DisposeHelper.NullSwapOrDispose(ref _layouts, layouts);
            }
            context.DrawBorder(borderBrush);
            return true;
        }

        public override void OnMouseDown(in MouseNotifyEventArgs args)
        {
            base.OnMouseDown(args);
            if (!args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            if (!Bounds.Contains(args.Location))
            {
                Close();
                return;
            }

            _isPressed = true;
            Update();
        }

        public override void OnMouseUp(in MouseNotifyEventArgs args)
        {
            base.OnMouseUp(args);
            if (!args.Buttons.HasFlagOptimized(MouseButtons.LeftButton) || !Bounds.Contains(args.Location))
                return;
            _isPressed = false;
            int hoveredIndex = _hoveredIndex;
            ContextMenuItem[] items = MenuItems;
            if (items.Length > hoveredIndex && hoveredIndex != -1)
            {
                ContextMenuItem item = items[hoveredIndex];
                ItemClicked?.Invoke(this, EventArgs.Empty);
                item.OnClick();
                Close();
            }
        }

        public override void OnMouseMove(in MouseNotifyEventArgs args)
        {
            base.OnMouseMove(args);

            Rectangle bounds = Bounds;
            int hoveredIndex;

            if (!bounds.Contains(args.Location))
                hoveredIndex = -1;
            else
            {
                float itemHeight = _itemHeight;
                hoveredIndex = (int)((args.Y - Location.Y) / itemHeight);
                if (MenuItems.Length <= hoveredIndex)
                    hoveredIndex = -1;
            }

            if (_hoveredIndex != hoveredIndex)
            {
                _hoveredIndex = hoveredIndex;
                Update();
            }
        }

        public void OnKeyDown(ref KeyInteractEventArgs args)
        {
            if (args.Key != VirtualKey.Escape ||
                Keys.IsAltPressed() || Keys.IsControlPressed() || Keys.IsShiftPressed())
                return;
            args.Handle();
            Close();
        }

        public void OnKeyUp(ref KeyInteractEventArgs args)
        {
            //Do nothing
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            DWriteTextLayout[]? layouts = InterlockedHelper.Read(ref _layouts);
            if (disposing)
            {
                DisposeHelper.DisposeAll(layouts);
                DisposeHelper.DisposeAll(_brushes);
            }
            if (layouts is not null)
                SequenceHelper.Clear(layouts);
            SequenceHelper.Clear(_brushes);
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
