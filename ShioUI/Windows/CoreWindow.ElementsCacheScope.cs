using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Utils;

namespace ShioUI.Windows;

partial class CoreWindow
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ElementsCacheScope : IDisposable
    {
        private CacheStore<UIElement?>.CacheNode? _node;

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _node!.Count; // throws NRE here if disposed
        }

        public readonly ulong Timestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _node!.Timestamp; // throws NRE here if disposed
        }

        public readonly UIElement? this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CacheStore<UIElement?>.CacheNode? node = _node;
                if (index < 0 || index >= node!.Count) // throws NRE here if disposed
                    IndexOutOfRangeException.Throw();
                UIElement?[]? array = node.Array;
                DebugHelper.ThrowIf(array is null);
                return array.AsUnsafeRef()[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ElementsCacheScope(CacheStore<UIElement?>.CacheNode node) => _node = node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(UIElement?[] destination)
        {
            CacheStore<UIElement?>.CacheNode? node = _node;
            UIElement?[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            Array.Copy(array, destination, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(UIElement?[] destination, int startIndex)
        {
            CacheStore<UIElement?>.CacheNode? node = _node;
            UIElement?[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            Array.Copy(array, 0, destination, startIndex, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly UIElement? GetReferenceOfFirstElement()
        {
            UIElement?[]? array = _node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return ref UnsafeHelper.GetArrayDataReference(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UIElement?[] ToArray()
        {
            CacheStore<UIElement?>.CacheNode? node = _node;
            UIElement?[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return UIElementHelper.CopyElementsToArrayUnsafe(ref UnsafeHelper.GetArrayDataReference(array), node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            CacheStore<UIElement?>.CacheNode? node = _node;
            UIElement?[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return new Enumerator(array, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            CacheStore<UIElement?>.CacheNode? node = _node;
            if (node is null)
                return;
            _node = node;
            node.Owner!.Dereference(node);
        }

        [StructLayout(LayoutKind.Auto)]
        public ref struct Enumerator : IEnumerator<UIElement?>
        {
            private UIElement?[]? _array;
            private int _count, _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UIElement?[] array, int count)
            {
                _array = array;
                _count = count;
                _index = -1;
            }

            public readonly UIElement? Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int index = _index;
                    if (index < 0 || index >= _count)
                        return InvalidOperationException.Throw<UIElement?>();
                    return _array!.AsUnsafeRef()[index];
                }
            }

            readonly object? IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _array = null;
                _index = -1;
                _count = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                int count = _count;
                if (index < count)
                {
                    _index = index;
                    return index >= 0;
                }
                return false;
            }

            public void Reset() => _index = -1;
        }
    }
}
