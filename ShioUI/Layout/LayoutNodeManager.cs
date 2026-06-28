using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Internals;
using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct LayoutNodeManager
{
    private readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> _elementDict;
    private readonly Dictionary<UIElement, ArraySegment<UIElement>> _childrenDict;
    private readonly Dictionary<UIElement, UIElement> _parentDict;
    private readonly Dictionary<LayoutNode, int>? _walkedNodes;
    private readonly Size _pageSize;
    private readonly ulong _timestamp;

    public LayoutNodeManager(
        Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict,
        Dictionary<UIElement, ArraySegment<UIElement>> childrenDict,
        Dictionary<UIElement, UIElement> parentDict,
        Size pageSize,
        ulong timestamp)
    {
        _elementDict = elementDict;
        _childrenDict = childrenDict;
        _parentDict = parentDict;
        _pageSize = pageSize;
        _timestamp = timestamp;
        if (ShioSettings.UseDebugMode)
            _walkedNodes = new Dictionary<LayoutNode, int>(LayoutNodeEqualityComparer.Instance);
        else
            _walkedNodes = null;
    }

    public readonly Size GetPageSize() => _pageSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ChildrenEnumerator GetChildrenEnumerator(UIElement element)
    {
        if (!_childrenDict.TryGetValue(element, out ArraySegment<UIElement> segment))
            return default;
        UIElement[]? array = segment.Array;
        int offset = segment.Offset;
        int count = segment.Count;
        if (array is null || offset < 0 || count <= 0)
            return default;
        return new ChildrenEnumerator(array, offset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetParentElement(UIElement element, [NotNullWhen(true)] out UIElement? parent)
        => _parentDict.TryGetValue(element, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<LayoutNode>(nameof(property));

        if (!_elementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
            return null;

        return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset + (int)property);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int?[] GetComputedValues(UIElement element)
    {
        int?[] result = ArrayHelper.CreateUninitializedArray<int?>((int)LayoutProperty._Last);
        ref int? resultRef = ref UnsafeHelper.GetArrayDataReference(result);

        if (!_elementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
        {
            Rect bounds = element.Bounds;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Left) = bounds.Left;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Top) = bounds.Top;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Right) = bounds.Right;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Bottom) = bounds.Bottom;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Width) = bounds.Width;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Height) = bounds.Height;
            return result;
        }

        ref LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);

        for (nuint property = (nuint)LayoutProperty.Left; property < (nuint)LayoutProperty._Last; property++)
        {
            LayoutNode? node = UnsafeHelper.AddTypedOffset(ref nodeRef, property);
            if (node is null)
                continue;
            UnsafeHelper.AddTypedOffset(ref resultRef, property) = GetComputedValue(node);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(UIElement element, LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<int>(nameof(property));

        if (!_elementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
        {
            return property switch
            {
                LayoutProperty.Left => element.Left,
                LayoutProperty.Top => element.Top,
                LayoutProperty.Right => element.Right,
                LayoutProperty.Bottom => element.Bottom,
                LayoutProperty.Width => element.Width,
                LayoutProperty.Height => element.Height,
                _ => ArgumentOutOfRangeException.Throw<int>(nameof(property))
            };
        }

        ref readonly LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffsetAsReadOnly(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);
        LayoutNode? node = UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)property);
        if (node is not null)
            return GetComputedValue(node);

        return property switch
        {
            LayoutProperty.Left => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Right)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Width)),
            LayoutProperty.Top => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Height)),
            LayoutProperty.Right => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Left)) +
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Width)),
            LayoutProperty.Bottom => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Top)) +
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Height)),
            LayoutProperty.Width => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Right)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Left)),
            LayoutProperty.Height => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Top)),
            _ => ArgumentOutOfRangeException.Throw<int>(nameof(property))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int GetComputedValueOrZero(LayoutNode? variable)
        => variable is null ? 0 : GetComputedValue(variable);

    public readonly int GetComputedValue(LayoutNode node)
    {
        if (node is FixedValueLayoutNode fixedValueNode)
            return fixedValueNode.Value;

        AddNodeOrThrow(node);
        try
        {
            return node.Compute(this, _timestamp);
        }
        finally
        {
            RemoveNode(node);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNodeOrThrow(LayoutNode node)
    {
        Dictionary<LayoutNode, int>? walkedNodes = _walkedNodes;
        if (walkedNodes is null)
            return;
        if (walkedNodes.ContainsKey(node))
            ThrowCyclicDependencyException(walkedNodes);
        else
            walkedNodes.Add(node, walkedNodes.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveNode(LayoutNode node) => _walkedNodes?.Remove(node);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCyclicDependencyException(Dictionary<LayoutNode, int> nodes)
        => throw new CyclicDependencyException(
            nodes.OrderBy(static pair => pair.Value)
            .Select(static pair => pair.Key)
            .ToArray()
            );

    public ref struct ChildrenEnumerator : IEnumerator<UIElement>
    {
        private readonly UIElement[] _array;
        private readonly int _offset;
        private readonly int _count;

        private int _index;

        public ChildrenEnumerator(UIElement[] array, int offset, int count)
        {
            _array = array;
            _offset = offset;
            _count = count;
            _index = -1;
        }

        public readonly UIElement Current
        {
            get
            {
                int index = _index;
                if (index < 0 || index >= _count)
                    return InvalidOperationException.Throw<UIElement>();
                return _array.AsUnsafeRef()[_offset + index];
            }
        }

        readonly object? IEnumerator.Current => Current;

        public readonly void Dispose() { }

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

        public void Reset()
        {
            _index = 0;
        }
    }
}
