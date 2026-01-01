using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public abstract partial class AppendOnlyListBase<TItem> : ScrollableElementBase where TItem : IAppendOnlyListItem<TItem>
    {
        protected readonly ItemStore _itemStore;

        protected AppendOnlyListBase(IRenderer renderer, string themePrefix, ItemStore itemStore) : base(renderer, themePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            Initialize();
        }

        protected AppendOnlyListBase(IRenderer renderer, string themePrefix, string scrollBarThemePrefix, ItemStore itemStore) : base(renderer, themePrefix, scrollBarThemePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            Initialize();
        }

        protected virtual void Initialize()
        {
            ScrollBarType = ScrollBarType.AutoVertial;
            StickBottom = true;
        }

        protected void Append(TItem item)
        {
            _itemStore.Append(item);
            Update();
        }

        protected override void OnContentBoundsChanged()
        {
            base.OnContentBoundsChanged();
            _itemStore.RecalculateAll(force: false);
        }

        protected override bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
        {
            if (context.HasDirtyCollector)
            {
                RenderBackground(context, backBrush);
                context.MarkAsDirty();
            }

            Rect bounds = ContentBounds;
            int viewportY = MathHelper.Max(ViewportPoint.Y, 0);
            ItemStore itemStore = _itemStore;
            int renderLeft = 0, renderRight = bounds.Width, boundsHeight = bounds.Height;
            while (itemStore.TryGetItem(viewportY, out TItem? item, out int itemTop, out int itemHeight))
            {
                int renderTop = itemTop - viewportY;
                int renderBottom = renderTop + itemHeight;
                RenderItem(in context, new RectF(renderLeft, renderTop, renderRight, renderBottom), item);
                if (renderBottom >= boundsHeight)
                    break;
                viewportY = itemTop + itemHeight + 1;
            }

            return true;
        }

        protected virtual void RenderItem(in RegionalRenderingContext context, RectF itemBounds, TItem item)
        {
            using RegionalRenderingContext clipContext = context.WithPixelAlignedClip(itemBounds, D2D1AntialiasMode.Aliased, out _);
            item.Render(clipContext, itemBounds.Size);
        }

        private void ItemStore_HeightChanged(ItemStore sender, int height)
        {
            SurfaceSize = new Size(0, height);
        }

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            _itemStore.Dispose();
        }
    }
}
