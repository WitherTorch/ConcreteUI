using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Layout;

[StructLayout(LayoutKind.Auto)]
public unsafe ref partial struct VirtualLayoutContext : ILayoutContext, IDisposable
{
    private readonly int _fakeLayoutNodeCount;
    private readonly PooledList<LayoutNode> _walkedNonCachedNodeList;

    private ArrayPool<LayoutNode>? _nodePool;
    private NativeMemoryPool? _memoryPool;
    private LayoutNode[]? _fakeLayoutNodeKeys;
    private LayoutContext _context;
    private int* _fakeLayoutNodeValues;
    private nuint _fakeLayoutNodeValuesLength;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal VirtualLayoutContext(scoped ref Builder builder)
    {
        PooledList<LayoutNode> walkedNonCachedNodeList = new PooledList<LayoutNode>(capacity: 0);
        _walkedNonCachedNodeList = walkedNonCachedNodeList;

        _nodePool = ReferenceHelper.Exchange(ref builder._nodePool, default);
        _memoryPool = ReferenceHelper.Exchange(ref builder._memoryPool, default);
        _fakeLayoutNodeValuesLength = ReferenceHelper.Exchange(ref builder._fakeLayoutNodeValuesLength, default);

        LayoutNode[]? fakeLayoutNodeKeys = ReferenceHelper.Exchange(ref builder._fakeLayoutNodeKeys, default);
        int* fakeLayoutNodeValues = ReferenceHelper.Exchange(ref builder._fakeLayoutNodeValues, default);
        int fakeLayoutNodeCount = ReferenceHelper.Exchange(ref builder._fakeLayoutNodeCount, default);

        _fakeLayoutNodeKeys = fakeLayoutNodeKeys;
        _fakeLayoutNodeValues = fakeLayoutNodeValues;
        _fakeLayoutNodeCount = fakeLayoutNodeCount;

        _context = new LayoutContext(
            ReferenceHelper.Exchange(ref builder._elementDict, default)!,
            ReferenceHelper.Exchange(ref builder._childrenDict, default)!,
            ReferenceHelper.Exchange(ref builder._parentDict, default)!,
            walkedNonCachedNodeList,
            fakeLayoutNodeKeys,
            fakeLayoutNodeValues,
            fakeLayoutNodeCount,
            builder._pageSize,
            builder._timestamp
            );
    }

    public readonly Size PageSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.PageSize;
    }

    public readonly ulong Timestamp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.Timestamp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Builder CreateVirtualContextBuilder() => _context.CreateVirtualContextBuilder();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutContext.ChildrenEnumerator GetChildrenEnumerator(UIElement element)
        => _context.GetChildrenEnumerator(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(LayoutNode node)
        => _context.GetComputedValue(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        => _context.GetComputedValue(element, property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int?[] GetComputedValues(UIElement element)
        => _context.GetComputedValues(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
        => _context.GetLayoutNodeOrNull(element, property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetParentElement(UIElement element, [NotNullWhen(true)] out UIElement? parent)
        => _context.TryGetParentElement(element, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ClearTemporaryCacheForNodes()
    {
        PooledList<LayoutNode> walkedNonCachedNodeList = _walkedNonCachedNodeList;
        foreach (LayoutNode node in walkedNonCachedNodeList)
            node.ClearCache();
        walkedNonCachedNodeList.Clear();
    }

    public void Dispose()
    {
        _context = default;
        _walkedNonCachedNodeList.Dispose();

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
