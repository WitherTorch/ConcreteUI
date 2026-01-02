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
            int renderLeft = 0, renderRight = bounds.Width, boundsHeight = bounds.Height;
            foreach ((TItem item, int itemTop, int itemHeight) in _itemStore.EnumerateItems(viewportY, boundsHeight))
            {
                int renderTop = itemTop - viewportY;
                int renderBottom = renderTop + itemHeight;
                RectF itemBounds = new RectF(renderLeft, renderTop, renderRight, renderBottom);
                using RegionalRenderingContext clipContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                RenderItem(in clipContext, item, itemBounds.Size);
            }

            return true;
        }

        protected virtual void RenderItem(in RegionalRenderingContext context, TItem item, SizeF renderSize) 
            => item.Render(context, renderSize);

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
