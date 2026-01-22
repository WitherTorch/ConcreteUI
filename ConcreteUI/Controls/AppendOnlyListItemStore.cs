using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public delegate void HeightChangedEventHandler<T>(object? sender, int height) where T : IAppendOnlyListItem<T>;
    public delegate void ItemRemovedEventHandler<T>(object? sender, T item) where T : IAppendOnlyListItem<T>;

    public sealed class AppendOnlyListItemStore<T> where T : IAppendOnlyListItem<T>
    {
        private readonly IAppendOnlyCollection<int> _keys;
        private readonly IAppendOnlyCollection<T> _values;
        private readonly object _syncLock;

        private ulong _disposed;

        public event HeightChangedEventHandler<T>? HeightChanged;
        public event ItemRemovedEventHandler<T>? ItemRemoved;

        public bool IsDisposed => InterlockedHelper.Read(ref _disposed) != 0UL;

        private AppendOnlyListItemStore(IAppendOnlyCollection<int> keys, IAppendOnlyCollection<T> values)
        {
            _keys = keys;
            _values = values;
            _syncLock = new object();
        }

        public static AppendOnlyListItemStore<T> CreateLimited(int capacity)
            => new AppendOnlyListItemStore<T>(
                keys: AppendOnlyCollection.CreateLimitedCollection<int>(capacity),
                values: AppendOnlyCollection.CreateLimitedCollection<T>(capacity));

        public static AppendOnlyListItemStore<T> CreateUnlimited()
            => new AppendOnlyListItemStore<T>(
                keys: AppendOnlyCollection.CreateUnlimitedCollection<int>(),
                values: AppendOnlyCollection.CreateUnlimitedCollection<T>());

        public void Append(T item)
        {
            if (IsDisposed)
                return;
            lock (_syncLock)
                AppendCore(item);
        }

        public void Append(IEnumerable<T> items)
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

        public bool TryGetItem(int height, [NotNullWhen(true)] out T? item, out int itemTop, out int itemHeight)
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

        public bool TryGetHeight(T item, out int itemTop, out int itemHeight)
        {
            if (IsDisposed)
                goto Failed;
            lock (_syncLock)
                return TryGetHeightCore_EqualityCompare(item, EqualityComparer<T>.Default, out itemTop, out itemHeight);
            Failed:
            itemHeight = 0;
            itemTop = 0;
            return false;
        }

        public bool TryGetHeight(T item, bool useBinarySearch, out int itemTop, out int itemHeight)
        {
            if (IsDisposed)
                goto Failed;
            lock (_syncLock)
            {
                if (useBinarySearch)
                    return TryGetHeightCore_BinarySearch(item, null, out itemTop, out itemHeight);
                else
                    return TryGetHeightCore_EqualityCompare(item, EqualityComparer<T>.Default, out itemTop, out itemHeight);
            }
        Failed:
            itemHeight = 0;
            itemTop = 0;
            return false;
        }

        public bool TryGetHeight(T item, bool useBinarySearch, IComparer<T> comparer, out int itemTop, out int itemHeight)
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

        public bool TryGetHeight(T item, IEqualityComparer<T> comparer, out int itemTop, out int itemHeight)
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

        public bool TryGetHeight(T item, IComparer<T> comparer, out int itemTop, out int itemHeight)
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

        public int EnumerateItemsToList(int baseY, int height, IList<(T item, int itemTop, int itemHeight)> list)
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

        private void AppendCore(T item) => HeightChanged?.Invoke(this, AppendCoreInternal(item));

        private void AppendCore(IEnumerable<T> items)
        {
            switch (items)
            {
                case T[] array:
                    AppendCoreInternal(array, array.Length);
                    break;
                case UnwrappableList<T> list:
                    AppendCoreInternal(list.Unwrap(), list.Count);
                    break;
                case IList<T> list:
                    AppendCoreInternal(list);
                    break;
                case IReadOnlyList<T> list:
                    AppendCoreInternal(list);
                    break;
                default:
                    AppendCoreInternal(items);
                    break;
            }
        }

        private void AppendCoreInternal(T[] items, int count)
        {
            if (count <= 0)
                return;
            ref T itemRef = ref items[0];
            int key, i = 0;
            do
            {
                key = AppendCoreInternal(UnsafeHelper.AddTypedOffset(ref itemRef, i));
            } while (++i < count);
            HeightChanged?.Invoke(this, key);
        }

        private void AppendCoreInternal(IList<T> items)
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

        private void AppendCoreInternal(IReadOnlyList<T> items)
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

        private void AppendCoreInternal(IEnumerable<T> items)
        {
            using IEnumerator<T> enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            int key;
            do
            {
                key = AppendCoreInternal(enumerator.Current);
            } while (enumerator.MoveNext());
            HeightChanged?.Invoke(this, key);
        }

        private int AppendCoreInternal(T item)
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

            int count = values.Count;

            int referenceKey;
            T? referenceValue;
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
                T headValue = values[0];
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
            IAppendOnlyCollection<T>? values = _values;

            int count = values.Count;
            if (count <= 0)
                return;
            T value = values[0];
            int key = value.CalculateHeight(reference: default, force);
            if (keys.Count == count)
            {
                keys[0] = key;
                for (int i = 1; i < count; i++)
                {
                    T reference = value;
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
                    T reference = value;
                    value = values[i];
                    key += value.CalculateHeight(reference, force);
                    keys.Append(key);
                }
            }
            if (triggerEvent)
                HeightChanged?.Invoke(this, key);
        }

        private bool TryGetItemCore(int height, [NotNullWhen(true)] out T? item, out int itemTop, out int itemHeight)
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

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

        private bool TryGetHeightCore_EqualityCompare<TEqualityComparer>(T item, TEqualityComparer comparer, out int itemTop, out int itemHeight)
            where TEqualityComparer : IEqualityComparer<T>
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

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

        private bool TryGetHeightCore_Compare<TComparer>(T item, TComparer comparer, out int itemTop, out int itemHeight)
            where TComparer : IComparer<T>
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

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

        private bool TryGetHeightCore_BinarySearch(T item, IComparer<T>? comparer, out int itemTop, out int itemHeight)
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

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

        private int EnumerateItemsCore(int startY, int endY, IList<(T item, int itemTop, int itemHeight)> list)
        {
            IAppendOnlyCollection<int>? keys = _keys;
            IAppendOnlyCollection<T>? values = _values;

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
            if (list is CustomListBase<(T item, int itemTop, int itemHeight)> customList)
                customList.EnsureCapacity(customList.Count + result);
#if NET8_0_OR_GREATER
                else if (list is List<(T item, int itemTop, int itemHeight)> normalList)
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
                IAppendOnlyCollection<T> values = _values;

                keys.Clear();
                foreach (T value in _values)
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
