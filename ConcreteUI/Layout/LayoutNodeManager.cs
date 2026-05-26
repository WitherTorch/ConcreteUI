using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Layout
{
    public readonly ref struct LayoutNodeManager
    {
        private readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> _elementDict;
        private readonly Dictionary<LayoutNode, int> _computeDict;
        private readonly Dictionary<LayoutNode, int>? _walkedNodes;
        private readonly Rect _pageRect;

        public LayoutNodeManager(in Rect pageRect,
            Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict,
            Dictionary<LayoutNode, int> computeDict)
        {
            _elementDict = elementDict;
            _computeDict = computeDict;
            _pageRect = pageRect;
            if (ConcreteSettings.UseDebugMode)
                _walkedNodes = new Dictionary<LayoutNode, int>();
            else
                _walkedNodes = null;
        }

        public readonly Rect GetPageRect() => _pageRect;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
        {
            if (property >= LayoutProperty._Last)
                return Throw();

            if (!_elementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
                return null;

            return UnsafeHelper.AddTypedOffset(in UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset + (int)property);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static LayoutNode? Throw() => throw new ArgumentOutOfRangeException(nameof(property));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int?[] GetComputedValues(UIElement element)
        {
            int?[] result = ArrayHelper.CreateUninitializedArray<int?>((int)LayoutProperty._Last);
            ref int? resultRef = ref UnsafeHelper.GetArrayDataReference(result);

            if (!_elementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
            {
                Rect bounds = element.Bounds;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Left) = bounds.Left;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Top) = bounds.Top;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Right) = bounds.Right;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Bottom) = bounds.Bottom;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Width) = bounds.Width;
                UnsafeHelper.AddTypedOffset(in resultRef, (nuint)LayoutProperty.Height) = bounds.Height;
                return result;
            }

            ref LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffset(in UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);

            for (nuint property = (nuint)LayoutProperty.Left; property < (nuint)LayoutProperty._Last; property++)
            {
                LayoutNode? node = UnsafeHelper.AddTypedOffset(in nodeRef, property);
                if (node is null)
                    continue;
                UnsafeHelper.AddTypedOffset(in resultRef, property) = GetComputedValue(node);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        {
            if (property >= LayoutProperty._Last)
                return Throw();

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
                    _ => Throw()
                };
            }

            ref LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffset(in UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);
            LayoutNode? node = UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)property);
            if (node is not null)
                return GetComputedValue(node);

            return property switch
            {
                LayoutProperty.Left => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Right)) -
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Width)),
                LayoutProperty.Top => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Height)),
                LayoutProperty.Right => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Left)) +
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Width)),
                LayoutProperty.Bottom => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Top)) +
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Height)),
                LayoutProperty.Width => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Right)) -
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Left)),
                LayoutProperty.Height => GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                    GetComputedValueOrZero(UnsafeHelper.AddTypedOffset(in nodeRef, (nuint)LayoutProperty.Top)),
                _ => Throw()
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Throw() => throw new ArgumentOutOfRangeException(nameof(property));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetComputedValueOrZero(LayoutNode? variable)
            => variable is null ? 0 : GetComputedValue(variable);

        public readonly int GetComputedValue(LayoutNode node)
        {
            if (node is FixedValueLayoutNode fixedValueNode)
                return fixedValueNode.Value;

            Dictionary<LayoutNode, int> computeDict = _computeDict;
            if (computeDict.TryGetValue(node, out int result))
                return result;
            AddNodeOrThrow(node);
            try
            {
                result = node.Compute(this);
            }
            finally
            {
                RemoveNode(node);
            }
            computeDict.Add(node, result);
            return result;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowCyclicDependencyException(Dictionary<LayoutNode, int> nodes)
            => throw new CyclicDependencyException(
                nodes.OrderBy(static pair => pair.Value)
                .Select(static pair => pair.Key)
                .ToArray()
                );
    }
}
