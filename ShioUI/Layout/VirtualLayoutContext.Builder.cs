using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Layout;

partial struct VirtualLayoutContext
{
    public unsafe ref struct Builder : IDisposable
    {
        internal readonly Size _pageSize;
        internal readonly ulong _timestamp;

        internal Dictionary<UIElement, ArraySegment<LayoutNode?>>? _elementDict;
        internal Dictionary<UIElement, ArraySegment<UIElement>>? _childrenDict;
        internal Dictionary<UIElement, UIElement>? _parentDict;
        internal ArrayPool<LayoutNode>? _nodePool;
        internal NativeMemoryPool? _memoryPool;
        internal LayoutNode[]? _fakeLayoutNodeKeys;
        internal int* _fakeLayoutNodeValues;
        internal nuint _fakeLayoutNodeValuesLength;
        internal int _fakeLayoutNodeCount;

        public Builder(scoped in LayoutContext context)
        {
            _elementDict = context._elementDict;
            _childrenDict = context._childrenDict;
            _parentDict = context._parentDict;
            _pageSize = context.PageSize;
            _timestamp = context.Timestamp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VirtualLayoutContext Build() => new VirtualLayoutContext(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFakeNodeValue(LayoutNode node, int value)
        {
            int count = _fakeLayoutNodeCount;
            if (count <= 0)
                SetFakeNodeValueFast(node, value);
            else
                SetFakeNodeValueSlow(node, value, count);
        }

        private void SetFakeNodeValueFast(LayoutNode node, int value)
        {
            DebugHelper.ThrowIf(_fakeLayoutNodeKeys is not null);
            DebugHelper.ThrowIf(_fakeLayoutNodeValues is not null);
            DebugHelper.ThrowIf(_fakeLayoutNodeValuesLength != 0);

            ArrayPool<LayoutNode> nodePool = ArrayPool<LayoutNode>.Shared;
            NativeMemoryPool memoryPool = NativeMemoryPool.Shared;

            LayoutNode[] keys = nodePool.Rent(1);
            _fakeLayoutNodeKeys = keys;

            DebugHelper.ThrowIf(keys.Length < 1);

            TypedNativeMemoryBlock<int> memoryBlock = memoryPool.Rent<int>(1);
            _memoryPool = memoryPool;

            int* values = memoryBlock.NativePointer;
            nuint valuesLength = memoryBlock.Length;

            DebugHelper.ThrowIf(valuesLength < 1);

            _fakeLayoutNodeValues = values;
            _fakeLayoutNodeValuesLength = valuesLength;

            keys.AsUnsafeRef()[0] = node;
            values[0] = value;

            _nodePool = nodePool;
            _fakeLayoutNodeCount = 1;
        }

        private void SetFakeNodeValueSlow(LayoutNode node, int value, int count)
        {
            LayoutNode[]? keys = _fakeLayoutNodeKeys;
            int* values = _fakeLayoutNodeValues;
            nuint valuesLength = _fakeLayoutNodeValuesLength;

            DebugHelper.ThrowIf(keys is null || keys.Length < count);
            DebugHelper.ThrowIf(values is null);
            DebugHelper.ThrowIf(valuesLength < (uint)count);

            int indexOf = Array.IndexOf(keys, node, 0, count);
            if (indexOf >= 0 && indexOf < count)
            {
                values[indexOf] = value;
                return;
            }

            int newCount = count + 1;
            if (keys.Length < newCount)
            {
                ArrayPool<LayoutNode>? nodePool = _nodePool;
                DebugHelper.ThrowIf(nodePool is null);

                LayoutNode[] newKeys = nodePool.Rent(newCount);
                DebugHelper.ThrowIf(newKeys.Length < newCount);
                Array.Copy(keys, newKeys, keys.Length);

                nodePool.Return(keys);
                _fakeLayoutNodeKeys = keys = newKeys;
            }
            if (valuesLength < (nuint)newCount)
            {
                NativeMemoryPool? memoryPool = _memoryPool;
                DebugHelper.ThrowIf(memoryPool is null);

                TypedNativeMemoryBlock<int> memoryBlock = memoryPool.Rent<int>((nuint)newCount);

                int* newValues = memoryBlock.NativePointer;
                nuint newValuesLength = memoryBlock.Length;
                DebugHelper.ThrowIf(memoryBlock.Length < (nuint)newCount);

                UnsafeHelper.CopyBlockUnaligned(newValues, values, valuesLength);
                memoryPool.Return(new TypedNativeMemoryBlock<int>(values, valuesLength));

                _fakeLayoutNodeValues = values = newValues;
                _fakeLayoutNodeValuesLength = valuesLength = newValuesLength;
            }

            keys.AsUnsafeRef()[count] = node;
            values[count] = value;
            _fakeLayoutNodeCount = newCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _elementDict = null;
            _parentDict = null;
            _childrenDict = null;

            ArrayPool<LayoutNode>? nodePool = ReferenceHelper.Exchange(ref _nodePool, null);
            if (nodePool is not null)
            {
                LayoutNode[]? keys = ReferenceHelper.Exchange(ref _fakeLayoutNodeKeys, null);
                DebugHelper.ThrowIf(keys is null);
                nodePool.Return(keys);
            }

            NativeMemoryPool? memoryPool = ReferenceHelper.Exchange(ref _memoryPool, null);
            if (memoryPool is not null)
            {
                int* values = ReferenceHelper.Exchange(ref _fakeLayoutNodeValues, null);
                DebugHelper.ThrowIf(values is null);
                memoryPool.Return(new NativeMemoryBlock(values, ReferenceHelper.Exchange(ref _fakeLayoutNodeValuesLength, default) * sizeof(int)));
            }
        }
    }
}

public static class VirtualLayoutContextBuilderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref VirtualLayoutContext.Builder WithFakeNodeValue(this ref VirtualLayoutContext.Builder builder, LayoutNode node, int value)
    {
        builder.SetFakeNodeValue(node, value);
        return ref builder;
    }
}
