using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class AppendOnlyListBase<TItem>
    {
        protected delegate void HeightChangedEventHandler(ItemStore sender, int height);
        protected delegate void ItemRemovedEventHandler(ItemStore sender, TItem item);
        protected delegate void ItemsRemovedEventHandler(ItemStore sender, IEnumerable<TItem> item);

        protected sealed class ItemStore : IDisposable
        {
            private readonly IAppendOnlyCollection<int> _keys;
            private readonly IAppendOnlyCollection<TItem> _values;
            private readonly object _syncLock;

            private ulong _disposed;

            public event HeightChangedEventHandler? HeightChanged;
            public event ItemRemovedEventHandler? ItemRemoved;
            public event ItemsRemovedEventHandler? ItemsRemoved;

            public bool IsDisposed => InterlockedHelper.Read(ref _disposed) != 0UL;

            private ItemStore(IAppendOnlyCollection<int> keys, IAppendOnlyCollection<TItem> values)
            {
                _keys = keys;
                _values = values;
                _syncLock = new object();
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
                if (IsDisposed)
                    return;
                lock (_syncLock)
                    AppendCore(item);
            }

            public void Append(IEnumerable<TItem> items)
            {
                if (IsDisposed)
                    return;
                lock (_syncLock)
                    AppendCore(items);
            }

            public void Clear()
            {
                if (IsDisposed)
                    return;
                lock (_syncLock)
                    ClearCore();
            }

            public void RecalculateAll(bool force)
            {
                if (IsDisposed)
                    return;
                lock (_syncLock)
                    RecalculateAllCore(force, triggerEvent: true);
            }

            public bool TryGetItem(int height, [NotNullWhen(true)] out TItem? item, out int itemTop, out int itemHeight)
            {
                if (height < 0 || IsDisposed)
                    goto Failed;
                lock (_syncLock)
                    return TryGetItemCore(height, out item, out itemTop, out itemHeight);
                Failed:
                item = default;
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public bool TryGetHeight(TItem item, out int itemTop, out int itemHeight)
            {
                if (IsDisposed)
                    goto Failed;
                lock (_syncLock)
                    return TryGetHeightCore_EqualityCompare(item, EqualityComparer<TItem>.Default, out itemTop, out itemHeight);
                Failed:
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public bool TryGetHeight(TItem item, bool useBinarySearch, out int itemTop, out int itemHeight)
            {
                if (IsDisposed)
                    goto Failed;
                lock (_syncLock)
                {
                    if (useBinarySearch)
                        return TryGetHeightCore_BinarySearch(item, null, out itemTop, out itemHeight);
                    else
                        return TryGetHeightCore_EqualityCompare(item, EqualityComparer<TItem>.Default, out itemTop, out itemHeight);
                }
            Failed:
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public bool TryGetHeight(TItem item, bool useBinarySearch, IComparer<TItem> comparer, out int itemTop, out int itemHeight)
            {
                if (IsDisposed)
                    goto Failed;
                lock (_syncLock)
                {
                    if (useBinarySearch)
                        return TryGetHeightCore_BinarySearch(item, comparer, out itemTop, out itemHeight);
                    else
                        return TryGetHeightCore_Compare(item, comparer, out itemTop, out itemHeight);
                }
            Failed:
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public bool TryGetHeight(TItem item, IEqualityComparer<TItem> comparer, out int itemTop, out int itemHeight)
            {
                if (IsDisposed)
                    goto Failed;
                lock (_syncLock)
                    return TryGetHeightCore_EqualityCompare(item, comparer, out itemTop, out itemHeight);
                Failed:
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public bool TryGetHeight(TItem item, IComparer<TItem> comparer, out int itemTop, out int itemHeight)
            {
                if (IsDisposed)
                    goto Failed;
                lock (_syncLock)
                    return TryGetHeightCore_Compare(item, comparer, out itemTop, out itemHeight);
                Failed:
                itemHeight = 0;
                itemTop = 0;
                return false;
            }

            public int EnumerateItemsToList(int baseY, int height, IList<(TItem item, int itemTop, int itemHeight)> list)
            {
                if (baseY < 0 || height < 0 || IsDisposed)
                    goto Failed;
                int endY = baseY + height;
                if (endY < 0)
                    goto Failed;
                lock (_syncLock)
                    return EnumerateItemsCore(baseY, endY, list);
                Failed:
                return 0;
            }

            private void AppendCore(TItem item) => HeightChanged?.Invoke(this, AppendCoreInternal(item));

            private void AppendCore(IEnumerable<TItem> items)
            {
                switch (items)
                {
                    case TItem[] array:
                        AppendCoreInternal(array, array.Length);
                        break;
                    case UnwrappableList<TItem> list:
                        AppendCoreInternal(list.Unwrap(), list.Count);
                        break;
                    case IList<TItem> list:
                        AppendCoreInternal(list);
                        break;
                    case IReadOnlyList<TItem> list:
                        AppendCoreInternal(list);
                        break;
                    default:
                        AppendCoreInternal(items);
                        break;
                }
            }

            private void AppendCoreInternal(TItem[] items, int count)
            {
                if (count <= 0)
                    return;
                ref TItem itemRef = ref items[0];
                int key, i = 0;
                do
                {
                    key = AppendCoreInternal(UnsafeHelper.AddTypedOffset(ref itemRef, i));
                } while (++i < count);
                HeightChanged?.Invoke(this, key);
            }

            private void AppendCoreInternal(IList<TItem> items)
            {
                int count = items.Count;
                if (count <= 0)
                    return;
                int key, i = 0;
                do
                {
                    key = AppendCoreInternal(items[i]);
                } while (++i < count);
                HeightChanged?.Invoke(this, key);
            }

            private void AppendCoreInternal(IReadOnlyList<TItem> items)
            {
                int count = items.Count;
                if (count <= 0)
                    return;
                int key, i = 0;
                do
                {
                    key = AppendCoreInternal(items[i]);
                } while (++i < count);
                HeightChanged?.Invoke(this, key);
            }

            private void AppendCoreInternal(IEnumerable<TItem> items)
            {
                using IEnumerator<TItem> enumerator = items.GetEnumerator();
                if (!enumerator.MoveNext())
                    return;
                int key;
                do
                {
                    key = AppendCoreInternal(enumerator.Current);
                } while (enumerator.MoveNext());
                HeightChanged?.Invoke(this, key);
            }

            private int AppendCoreInternal(TItem item)
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
                int capacity = values.Capacity;
                if (capacity < count)
                {
                    // WTF?
                    throw new InvalidOperationException("The capacity of the list cannot lower than the count of the list!");
                }
                else if (capacity == count)
                {
                    int keyHead = keys[0];
                    keys.Append(key);
                    for (int i = 0; i < count; i++)
                    {
                        key = keys[i];
                        if (key < keyHead) // KeyList is broken, need rebuild keys
                        {
                            RecalculateAllCore(force: true, triggerEvent: false);
                            break;
                        }
                        key -= keyHead;
                        keys[i] = key;
                    }
                    TItem headValue = values[0];
                    values.Append(item);
                    ItemRemoved?.Invoke(this, headValue);
                    headValue.Dispose();
                }
                else
                {
                    keys.Append(key);
                    values.Append(item);
                }
                return key;
            }

            private void ClearCore()
            {
                _keys.Clear();
                _values.Clear();
                HeightChanged?.Invoke(this, 0);
            }

            private void RecalculateAllCore(bool force, bool triggerEvent)
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
                if (triggerEvent)
                    HeightChanged?.Invoke(this, key);
            }

            private bool TryGetItemCore(int height, [NotNullWhen(true)] out TItem? item, out int itemTop, out int itemHeight)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true, triggerEvent: true);
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

            private bool TryGetHeightCore_EqualityCompare<TEqualityComparer>(TItem item, TEqualityComparer comparer, out int itemTop, out int itemHeight)
                where TEqualityComparer : IEqualityComparer<TItem>
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true, triggerEvent: true);
                    count = keys.Count;
                }
                if (count == 0)
                    goto Failed;

                if (comparer.Equals(values[0], item))
                {
                    itemTop = 0;
                    itemHeight = keys[0];
                    return true;
                }

                for (int i = 1; i < count; i++)
                {
                    if (!comparer.Equals(values[i], item))
                        continue;
                    itemTop = keys[i - 1];
                    itemHeight = keys[i] - itemTop;
                    return true;
                }

                goto Failed;
            Failed:
                itemTop = 0;
                itemHeight = 0;
                return false;
            }

            private bool TryGetHeightCore_Compare<TComparer>(TItem item, TComparer comparer, out int itemTop, out int itemHeight)
                where TComparer : IComparer<TItem>
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true, triggerEvent: true);
                    count = keys.Count;
                }
                if (count == 0)
                    goto Failed;

                if (comparer.Compare(values[0], item) == 0)
                {
                    itemTop = 0;
                    itemHeight = keys[0];
                    return true;
                }

                for (int i = 1; i < count; i++)
                {
                    if (comparer.Compare(values[i], item) != 0)
                        continue;
                    itemTop = keys[i - 1];
                    itemHeight = keys[i] - itemTop;
                    return true;
                }

                goto Failed;
            Failed:
                itemTop = 0;
                itemHeight = 0;
                return false;
            }

            private bool TryGetHeightCore_BinarySearch(TItem item, IComparer<TItem>? comparer, out int itemTop, out int itemHeight)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true, triggerEvent: true);
                    count = keys.Count;
                }
                if (count == 0)
                    goto Failed;

                int index;
                if (comparer is null)
                    index = values.BinarySearch(item);
                else
                    index = values.BinarySearch(item, comparer);
                if (index < 0 || index >= count)
                    goto Failed;

                if (index == 0)
                {
                    itemTop = 0;
                    itemHeight = keys[index];
                }
                else
                {
                    itemTop = keys[index - 1];
                    itemHeight = keys[index] - itemTop;
                }
                return true;

            Failed:
                itemTop = 0;
                itemHeight = 0;
                return false;
            }

            private int EnumerateItemsCore(int startY, int endY, IList<(TItem item, int itemTop, int itemHeight)> list)
            {
                IAppendOnlyCollection<int>? keys = _keys;
                IAppendOnlyCollection<TItem>? values = _values;

                int count = keys.Count;
                if (count != values.Count)
                {
                    RecalculateAllCore(force: true, triggerEvent: true);
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

                int result = endIndex - startIndex + 1;
                if (list is CustomListBase<(TItem item, int itemTop, int itemHeight)> customList)
                    customList.EnsureCapacity(customList.Count + result);
#if NET8_0_OR_GREATER
                else if (list is List<(TItem item, int itemTop, int itemHeight)> normalList)
                    normalList.EnsureCapacity(normalList.Count + result);
#endif
                int key = 0;
                for (int i = startIndex, j = 0; i <= endIndex; i++, j++)
                {
                    int newKey = keys[i];
                    list.Add((values[i], key, newKey - key));
                    key = newKey;
                }

                return result;
            Failed:
                return 0;
            }

            private void DisposeCore()
            {
                if (InterlockedHelper.Exchange(ref _disposed, ulong.MaxValue) != 0UL)
                    return;
                lock (_syncLock)
                {
                    IAppendOnlyCollection<int> keys = _keys;
                    IAppendOnlyCollection<TItem> values = _values;

                    keys.Clear();
                    foreach (TItem value in _values)
                        value.Dispose();
                    values.Clear();
                }
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }
        }
    }
}
