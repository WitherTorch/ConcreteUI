using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using WitherTorch.Common.Collections;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Layout
{
    public readonly struct LayoutVariableManager
    {
        private readonly TreeDictionary<UIElement, LayoutVariable?[]> _elementDict;
        private readonly TreeDictionary<LayoutVariable, StrongBox<int?>> _computeDict;
        private readonly Rect _pageRect;

        public LayoutVariableManager(in Rect pageRect, TreeDictionary<UIElement, LayoutVariable?[]> elementDict, TreeDictionary<LayoutVariable, StrongBox<int?>> computeDict)
        {
            _elementDict = elementDict;
            _computeDict = computeDict;
            _pageRect = pageRect;
        }

        public readonly Rect GetPageRect() => _pageRect;

        public readonly LayoutVariable? GetVariable(UIElement element, LayoutProperty property)
            => _elementDict[element]?[(int)property];

        public readonly int?[] GetComputedValues(UIElement element)
        {
            LayoutVariable?[]? variables = _elementDict[element];
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
                LayoutVariable? variable = variables[(int)property];
                if (variable is null)
                    continue;
                result[(int)property] = GetComputedValue(variable);
            }
            return result;
        }

        public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        {
            LayoutVariable?[]? variables = _elementDict[element];
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
            LayoutVariable? variable = variables[(int)property];
            if (variable is not null)
                return GetComputedValue(variable);
            return property switch
            {
                LayoutProperty.Left => GetComputedValueChecked(variables[(int)LayoutProperty.Right]) - GetComputedValueChecked(variables[(int)LayoutProperty.Width]),
                LayoutProperty.Top => GetComputedValueChecked(variables[(int)LayoutProperty.Bottom]) - GetComputedValueChecked(variables[(int)LayoutProperty.Height]),
                LayoutProperty.Right => GetComputedValueChecked(variables[(int)LayoutProperty.Left]) + GetComputedValueChecked(variables[(int)LayoutProperty.Width]),
                LayoutProperty.Bottom => GetComputedValueChecked(variables[(int)LayoutProperty.Top]) + GetComputedValueChecked(variables[(int)LayoutProperty.Height]),
                LayoutProperty.Height => GetComputedValueChecked(variables[(int)LayoutProperty.Bottom]) - GetComputedValueChecked(variables[(int)LayoutProperty.Top]),
                LayoutProperty.Width => GetComputedValueChecked(variables[(int)LayoutProperty.Right]) - GetComputedValueChecked(variables[(int)LayoutProperty.Left]),
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetComputedValueChecked(LayoutVariable? variable)
            => variable is null ? 0 : GetComputedValue(variable);

        public readonly int GetComputedValue(LayoutVariable variable)
        {
            if (variable is FixedLayoutVariable fixedVariable)
                return fixedVariable.Value;

            TreeDictionary<LayoutVariable, StrongBox<int?>> computeDict = _computeDict;
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
