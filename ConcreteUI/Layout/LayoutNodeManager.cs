using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Layout
{
    public readonly struct LayoutNodeManager
    {
        private readonly TreeDictionary<UIElement, LayoutNode?[]> _elementDict;
        private readonly TreeDictionary<LayoutNode, StrongBox<int?>> _computeDict;
        private readonly Rect _pageRect;

        public LayoutNodeManager(in Rect pageRect, TreeDictionary<UIElement, LayoutNode?[]> elementDict, TreeDictionary<LayoutNode, StrongBox<int?>> computeDict)
        {
            _elementDict = elementDict;
            _computeDict = computeDict;
            _pageRect = pageRect;
        }

        public readonly Rect GetPageRect() => _pageRect;

        public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
            => _elementDict[element]?[(int)property];

        public readonly int?[] GetComputedValues(UIElement element)
        {
            LayoutNode?[]? variables = _elementDict[element];
            int?[] result = new int?[(int)LayoutProperty._Last];
            if (variables is null)
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
                LayoutNode? variable = variables[(int)property];
                if (variable is null)
                    continue;
                result[(int)property] = GetComputedValue(variable);
            }
            return result;
        }

        public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        {
            LayoutNode?[]? variables = _elementDict[element];
            if (variables is null)
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
            LayoutNode? variable = variables[(int)property];
            if (variable is not null)
                return GetComputedValue(variable);
            return property switch
            {
                LayoutProperty.Left => GetComputedValueOrZero(variables[(int)LayoutProperty.Right]) - GetComputedValueOrZero(variables[(int)LayoutProperty.Width]),
                LayoutProperty.Top => GetComputedValueOrZero(variables[(int)LayoutProperty.Bottom]) - GetComputedValueOrZero(variables[(int)LayoutProperty.Height]),
                LayoutProperty.Right => GetComputedValueOrZero(variables[(int)LayoutProperty.Left]) + GetComputedValueOrZero(variables[(int)LayoutProperty.Width]),
                LayoutProperty.Bottom => GetComputedValueOrZero(variables[(int)LayoutProperty.Top]) + GetComputedValueOrZero(variables[(int)LayoutProperty.Height]),
                LayoutProperty.Height => GetComputedValueOrZero(variables[(int)LayoutProperty.Bottom]) - GetComputedValueOrZero(variables[(int)LayoutProperty.Top]),
                LayoutProperty.Width => GetComputedValueOrZero(variables[(int)LayoutProperty.Right]) - GetComputedValueOrZero(variables[(int)LayoutProperty.Left]),
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetComputedValueOrZero(LayoutNode? variable)
            => variable is null ? 0 : GetComputedValue(variable);

        public readonly int GetComputedValue(LayoutNode variable)
        {
            if (variable is FixedValueLayoutNode fixedVariable)
                return fixedVariable.Value;

            TreeDictionary<LayoutNode, StrongBox<int?>> computeDict = _computeDict;
            int result;
            StrongBox<int?>? value = computeDict[variable];
            if (value is null)
            {
                result = variable.Compute(this);
                computeDict[variable] = new StrongBox<int?>(result);
                return result;
            }
            int? unboxedValue = value.Value;
            if (unboxedValue.HasValue)
                return unboxedValue.Value;
            result = variable.Compute(this);
            value.Value = result;
            return result;
        }
    }
}
