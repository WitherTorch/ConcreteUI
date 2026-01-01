using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using WitherTorch.Common.Collections;

namespace ConcreteUI.Controls
{
    partial class AppendOnlyListBase<TItem>
    {
        protected delegate void HeightChangedEventHandler(ItemStore sender, int height);

        protected sealed class ItemStore : IDisposable
        {
            private readonly IAppendOnlyCollection<int> _keys;
            private readonly IAppendOnlyCollection<TItem> _values;
            private readonly SemaphoreSlim _semaphore;

            private bool _disposed;

            public event HeightChangedEventHandler? HeightChanged;

            private ItemStore(IAppendOnlyCollection<int> keys, IAppendOnlyCollection<TItem> values)
            {
                _keys = keys;
                _values = values;
                _semaphore = new SemaphoreSlim(1, 1);
            }

            public static ItemStore CreateLimited(int capacity)
                => new ItemStore(
                    keys: AppendOnlyCollection.CreateLimitedCollection<int>(capacity),
                    values: AppendOnlyCollection.CreateLimitedCollection<TItem>(capacity));

            public static ItemStore CreateUnlimited()
                => new ItemStore(
                    keys: AppendOnlyCollection.CreateUnlimitedCollection<int>(),
                    values: AppendOnlyCollection.CreateUnlimitedCollection<TItem>());

            public void Append(TItem item)
            {
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    AppendCore(item);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            public void Clear()
            {
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    ClearCore();
                }
                finally
                {
                    semaphore.Release();
                }
            }

            public void RecalculateAll(bool force)
            {
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    RecalculateAllCore(force);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            public bool TryGetItem(int height, [NotNullWhen(true)] out TItem? item, out int itemTop, out int itemHeight)
            {
                if (height < 0)
                {
                    item = default;
                    itemHeight = 0;
                    itemTop = 0;
                    return false;
                }
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    return TryGetItemCore(height, out item, out itemTop, out itemHeight);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            public IEnumerable<(TItem item, int itemTop, int itemHeight)> EnumerateItems(int baseY, int height)
            {
                if (baseY < 0 || height < 0)
                    goto Failed;
                int endY = baseY + height;
                if (endY < 0)
                    goto Failed;
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    return EnumerateItemsCore(baseY, endY);
                }
                finally
                {
                    semaphore.Release();
                }
            Failed:
                return Enumerable.Empty<(TItem item, int itemTop, int itemHeight)>();
            }

            private void AppendCore(TItem item)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = values.Count;

                int referenceKey;
                TItem? referenceValue;
                if (count <= 0)
                {
                    referenceKey = 0;
                    referenceValue = default;
                }
                else
                {
                    int lastIndex = count - 1;
                    referenceKey = keys[lastIndex];
                    referenceValue = values[lastIndex];
                }
                int height = item.CalculateHeight(referenceValue, force: true);
                if (height < 0)
                    throw new InvalidOperationException("The item's height cannot be negative!");
                int key = referenceKey + height;
                values.Append(item);
                if (keys.Capacity <= count)
                {
                    int keyHead = keys[0];
                    keys.Append(key);
                    for (int i = 0; i < count; i++)
                    {
                        key = keys[i];
                        if (key < keyHead) // KeyList is broken, need rebuild keys
                        {
                            RecalculateAllCore(force: true);
                            break;
                        }
                        key -= keyHead;
                        keys[i] = key;
                    }
                }
                else
                {
                    keys.Append(key);
                }
                HeightChanged?.Invoke(this, key);
            }

            private void ClearCore()
            {
                _keys.Clear();
                _values.Clear();
                HeightChanged?.Invoke(this, 0);
            }

            private void RecalculateAllCore(bool force)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = values.Count;
                if (count <= 0)
                    return;
                TItem value = values[0];
                int key = value.CalculateHeight(reference: default, force);
                if (keys.Count == count)
                {
                    keys[0] = key;
                    for (int i = 1; i < count; i++)
                    {
                        TItem reference = value;
                        value = values[i];
                        key += value.CalculateHeight(reference, force);
                        keys[i] = key;
                    }
                }
                else
                {
                    keys.Clear();
                    keys.Append(key);
                    for (int i = 1; i < count; i++)
                    {
                        TItem reference = value;
                        value = values[i];
                        key += value.CalculateHeight(reference, force);
                        keys.Append(key);
                    }
                }
            }

            private bool TryGetItemCore(int height, [NotNullWhen(true)] out TItem? item, out int itemTop, out int itemHeight)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true);
                    count = keys.Count;
                }
                int index = keys.BinarySearch(height);
                if (index < 0)
                    index = ~index;
                if (index >= count)
                    goto Failed;
                item = values[index];
                itemTop = index > 0 ? keys[index - 1] : 0;
                itemHeight = keys[index] - itemTop;
                return true;

            Failed:
                item = default;
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public IEnumerable<(TItem item, int itemTop, int itemHeight)> EnumerateItems(int startY, int endY)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true);
                    count = keys.Count;
                }

                int startIndex = keys.BinarySearch(startY);
                if (startIndex < 0)
                {
                    startIndex = ~startIndex;
                    if (startIndex >= count)
                        goto Failed;
                    if (startIndex > 0)
                        startIndex--;
                }
                else
                {
                    if (startIndex >= count)
                        goto Failed;
                }

                int endIndex = keys.BinarySearch(endY);
                if (endIndex < 0)
                    endIndex = ~endIndex;
                if (endIndex >= count)
                    endIndex = count - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {

                }

            Failed:
                yield break;
            }

            private void DisposeCore()
            {
                if (_disposed)
                    return;
                _disposed = true;

                IAppendOnlyCollection<int> keys = _keys;
                IAppendOnlyCollection<TItem> values = _values;

                keys.Clear();
                foreach (TItem value in _values)
                    value.Dispose();
                values.Clear();
            }

            public void Dispose()
            {
                SemaphoreSlim semaphore = _semaphore;
                semaphore.Wait();
                try
                {
                    DisposeCore();
                }
                finally
                {
                    semaphore.Release();
                    semaphore.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }
    }
}
