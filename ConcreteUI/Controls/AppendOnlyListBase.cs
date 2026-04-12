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
    public abstract partial class AppendOnlyListBase<TItem, TMeasuringContext> : ScrollableElementBase
        where TItem : IMeasurableListItem<TMeasuringContext> where TMeasuringContext : IMeasuringContext
    {
        protected readonly AppendOnlyListItemStore<TItem, TMeasuringContext> _itemStore;
        private bool _ignoreNeedRefresh;

        protected AppendOnlyListBase(IElementContainer parent, string themePrefix, AppendOnlyListItemStore<TItem, TMeasuringContext> itemStore) : base(parent, themePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            itemStore.Bind(this);
            Initialize();
        }

        protected AppendOnlyListBase(IElementContainer parent, string themePrefix, string scrollBarThemePrefix, AppendOnlyListItemStore<TItem, TMeasuringContext> itemStore) : base(parent, themePrefix, scrollBarThemePrefix)
        {
            _itemStore = itemStore;
            itemStore.HeightChanged += ItemStore_HeightChanged;
            itemStore.Bind(this);
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
            _itemStore.AdjustAll();
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

            (TItem item, int itemTop, int itemHeight)[] array; int count;
            ArrayPool<(TItem item, int itemTop, int itemHeight)> pool = ArrayPool<(TItem item, int itemTop, int itemHeight)>.Shared;
            using (PooledList<(TItem item, int itemTop, int itemHeight)> list = new(pool, capacity: 0))
            {
                _itemStore.EnumerateItemsToList(viewportY, boundsHeight, list);
                (array, count) = list;
            }
            try
            {
                if (count <= 0)
                    return true;
                ref (TItem item, int itemTop, int itemHeight) itemRef = ref UnsafeHelper.GetArrayDataReference(array);
                int i = 0;
                do
                {
                    (TItem item, int itemTop, int itemHeight) = UnsafeHelper.AddTypedOffset(ref itemRef, i);
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
                } while (++i < count);
            }
            finally
            {
                pool.Return(array);
            }

            return true;
        }

        protected virtual void RenderItem(in RegionalRenderingContext context, TItem item, SizeF renderSize)
            => item.Render(context, renderSize);

        internal protected abstract TMeasuringContext CreateMeasuringContext();

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
