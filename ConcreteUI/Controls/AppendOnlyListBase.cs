using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public abstract partial class AppendOnlyListBase<TItem> : ScrollableElementBase where TItem : IAppendOnlyListItem<TItem>
    {
        protected readonly AppendOnlyListItemStore<TItem> _itemStore;
        private bool _ignoreNeedRefresh;

        protected AppendOnlyListBase(IRenderer renderer, string themePrefix, AppendOnlyListItemStore<TItem> itemStore) : base(renderer, themePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            Initialize();
        }

        protected AppendOnlyListBase(IRenderer renderer, string themePrefix, string scrollBarThemePrefix, AppendOnlyListItemStore<TItem> itemStore) : base(renderer, themePrefix, scrollBarThemePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            Initialize();
        }

        protected virtual void Initialize()
        {
            ScrollBarType = ScrollBarType.AutoVertial;
            StickBottom = true;
            _ignoreNeedRefresh = true;
        }

        protected virtual void Append(TItem item)
        {
            _itemStore.Append(item);
            Update();
        }

        protected virtual void Append(IEnumerable<TItem> items)
        {
            _itemStore.Append(items);
            Update();
        }

        protected override void OnContentBoundsChanged()
        {
            base.OnContentBoundsChanged();
            _itemStore.RecalculateAll(force: false);
        }

        protected override bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
        {
            bool ignoreNeedRefresh = ReferenceHelper.Exchange(ref _ignoreNeedRefresh, false);
            bool needRenderBackgroundPerItem;
            if (context.HasDirtyCollector)
            {
                if (ignoreNeedRefresh)
                {
                    needRenderBackgroundPerItem = false;
                    RenderBackground(context, backBrush);
                    context.MarkAsDirty();
                }
                else
                {
                    needRenderBackgroundPerItem = true;
                }
            }
            else
            {
                needRenderBackgroundPerItem = false;
                ignoreNeedRefresh = true;
            }

            Rect bounds = ContentBounds;
            int viewportY = MathHelper.Max(ViewportPoint.Y, 0);
            int renderLeft = 0, renderRight = bounds.Width, boundsHeight = bounds.Height;
            using PooledList<(TItem item, int itemTop, int itemHeight)> list = new PooledList<(TItem item, int itemTop, int itemHeight)>(capacity: 0);
            _itemStore.EnumerateItemsToList(viewportY, boundsHeight, list);
            for (int i = 0, count = list.Count; i < count; i++)
            {
                (TItem item, int itemTop, int itemHeight) = list[i];
                if (!ignoreNeedRefresh && !item.NeedRefresh())
                    continue;
                int renderTop = itemTop - viewportY;
                int renderBottom = renderTop + itemHeight;
                RectF itemBounds = new RectF(renderLeft, renderTop, renderRight, renderBottom);
                using RegionalRenderingContext clipContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                if (needRenderBackgroundPerItem)
                {
                    RenderBackground(clipContext, backBrush);
                    clipContext.MarkAsDirty();
                }
                RenderItem(in clipContext, item, itemBounds.Size);
            }

            return true;
        }

        protected virtual void RenderItem(in RegionalRenderingContext context, TItem item, SizeF renderSize)
            => item.Render(context, renderSize);

        private void ItemStore_HeightChanged(object? sender, int height)
        {
            _ignoreNeedRefresh = true;
            SurfaceSize = new Size(0, height);
        }

        public override void OnViewportPointChanged()
        {
            base.OnViewportPointChanged();
            _ignoreNeedRefresh = true;
        }

        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            _itemStore.Dispose();
        }
    }
}
