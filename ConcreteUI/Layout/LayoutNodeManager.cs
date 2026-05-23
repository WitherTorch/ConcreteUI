using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Layout
{
    public readonly ref struct LayoutNodeManager
    {
        private readonly TreeDictionary<UIElement, LayoutNode?[]> _elementDict;
        private readonly TreeDictionary<LayoutNode, StrongBox<int?>> _computeDict;
        private readonly Dictionary<LayoutNode, int>? _walkedNodes;
        private readonly Rect _pageRect;

        public LayoutNodeManager(in Rect pageRect, TreeDictionary<UIElement, LayoutNode?[]> elementDict, TreeDictionary<LayoutNode, StrongBox<int?>> computeDict)
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

        public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
            => _elementDict[element]?[(int)property];

        public readonly int?[] GetComputedValues(UIElement element)
        {
            LayoutNode?[]? nodes = _elementDict[element];
            int?[] result = new int?[(int)LayoutProperty._Last];
            if (nodes is null)
            {
                Rect bounds = element.Bounds;
                result[(int)LayoutProperty.Left] = bounds.Left;
                result[(int)LayoutProperty.Top] = bounds.Top;
                result[(int)LayoutProperty.Right] = bounds.Right;
                result[(int)LayoutProperty.Bottom] = bounds.Bottom;
                return result;
            }
            for (LayoutProperty property = LayoutProperty.Left; property < LayoutProperty._Last; property++)
            {
                LayoutNode? variable = nodes[(int)property];
                if (variable is null)
                    continue;
                result[(int)property] = GetComputedValue(variable);
            }
            return result;
        }

        public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        {
            LayoutNode?[]? nodes = _elementDict[element];
            if (nodes is null)
            {
                Rect bounds = element.Bounds;
                return property switch
                {
                    LayoutProperty.Left => bounds.Left,
                    LayoutProperty.Top => bounds.Top,
                    LayoutProperty.Right => bounds.Right,
                    LayoutProperty.Bottom => bounds.Bottom,
                    LayoutProperty.Height => bounds.Height,
                    LayoutProperty.Width => bounds.Width,
                    _ => 0
                };
            }
            LayoutNode? node = nodes[(int)property];
            if (node is not null)
                return GetComputedValue(node);
            return property switch
            {
                LayoutProperty.Left => GetComputedValueOrZero(nodes[(int)LayoutProperty.Right]) - GetComputedValueOrZero(nodes[(int)LayoutProperty.Width]),
                LayoutProperty.Top => GetComputedValueOrZero(nodes[(int)LayoutProperty.Bottom]) - GetComputedValueOrZero(nodes[(int)LayoutProperty.Height]),
                LayoutProperty.Right => GetComputedValueOrZero(nodes[(int)LayoutProperty.Left]) + GetComputedValueOrZero(nodes[(int)LayoutProperty.Width]),
                LayoutProperty.Bottom => GetComputedValueOrZero(nodes[(int)LayoutProperty.Top]) + GetComputedValueOrZero(nodes[(int)LayoutProperty.Height]),
                LayoutProperty.Height => GetComputedValueOrZero(nodes[(int)LayoutProperty.Bottom]) - GetComputedValueOrZero(nodes[(int)LayoutProperty.Top]),
                LayoutProperty.Width => GetComputedValueOrZero(nodes[(int)LayoutProperty.Right]) - GetComputedValueOrZero(nodes[(int)LayoutProperty.Left]),
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetComputedValueOrZero(LayoutNode? variable)
            => variable is null ? 0 : GetComputedValue(variable);

        public readonly int GetComputedValue(LayoutNode node)
        {
            if (node is FixedValueLayoutNode fixedValueNode)
                return fixedValueNode.Value;

            TreeDictionary<LayoutNode, StrongBox<int?>> computeDict = _computeDict;
            int result;
            StrongBox<int?>? value = computeDict[node];
            if (value is null)
            {
                AddNodeOrThrow(node);
                try
                {
                    result = node.Compute(this);
                }
                finally
                {
                    RemoveNode(node);
                }
                computeDict[node] = new StrongBox<int?>(result);
                return result;
            }
            int? unboxedValue = value.Value;
            if (unboxedValue.HasValue)
                return unboxedValue.Value;
            AddNodeOrThrow(node);
            try
            {
                result = node.Compute(this);
            }
            finally
            {
                RemoveNode(node);
            }
            value.Value = result;
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
