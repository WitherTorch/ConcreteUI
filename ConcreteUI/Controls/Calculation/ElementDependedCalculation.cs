using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

using InlineMethod;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class ElementDependedCalculation : AbstractCalculation
    {
        private readonly WeakReference<UIElement> _dependElementRef;
        private readonly LayoutProperty _dependProperty;
        private readonly Func<int, int> _func;

        private ElementDependedCalculation(WeakReference<UIElement> dependRef, LayoutProperty property, Func<int, int> func)
        {
            _dependElementRef = dependRef;
            _dependProperty = property;
            _func = func;
        }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property)
            : this(depend, property, AutoConfigureCalculation(property)) { }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property, int offset)
            : this(depend, property, AutoConfigureCalculation(property, offset)) { }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property, OffsetType offsetType)
            : this(depend, property, AutoConfigureCalculation(property, offsetType)) { }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property, OffsetType offsetType, int offset)
            : this(depend, property, AutoConfigureCalculation(property, offsetType, offset)) { }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property, Func<int, int> func)
            : this(new WeakReference<UIElement>(depend), property, func) { }

        [Inline(InlineBehavior.Remove)]
        private static Func<int, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property)
            => property switch
            {
                LayoutProperty.Left or LayoutProperty.Top => static val => val + UIConstants.ElementMargin,
                LayoutProperty.Right or LayoutProperty.Bottom => static val => val - UIConstants.ElementMargin,
                _ => static val => val,
            };

        [Inline(InlineBehavior.Remove)]
        private static Func<int, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property, [InlineParameter] int offset)
        {
            if (offset == 0)
                return static val => val;
            if (offset == UIConstants.ElementMargin)
                return property switch
                {
                    LayoutProperty.Right or LayoutProperty.Bottom => static val => val - UIConstants.ElementMargin,
                    _ => static val => val + UIConstants.ElementMargin,
                };
            return property switch
            {
                LayoutProperty.Right or LayoutProperty.Bottom => val => val - offset,
                _ => val => val + offset,
            };
        }

        [Inline(InlineBehavior.Remove)]
        private static Func<int, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property, [InlineParameter] OffsetType offsetType)
            => offsetType switch
            {
                OffsetType.Add => static val => val + UIConstants.ElementMargin,
                OffsetType.Subtract => static val => val - UIConstants.ElementMargin,
                OffsetType.Inside => AutoConfigureCalculation(property),
                OffsetType.Outside => property switch
                {
                    LayoutProperty.Left or LayoutProperty.Top => static val => val - UIConstants.ElementMargin,
                    LayoutProperty.Right or LayoutProperty.Bottom => static val => val + UIConstants.ElementMargin,
                    _ => static val => val,
                },
                OffsetType.None => static val => val,
                _ => throw new ArgumentOutOfRangeException(nameof(offsetType)),
            };

        [Inline(InlineBehavior.Remove)]
        private static Func<int, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property, [InlineParameter] OffsetType offsetType, [InlineParameter] int offset)
        {
            if (offset == 0)
                return static val => val;
            if (offset == UIConstants.ElementMargin)
                return AutoConfigureCalculation(property, offsetType);
            return offsetType switch
            {
                OffsetType.Add => val => val + offset,
                OffsetType.Subtract => val => val - offset,
                OffsetType.Inside => property switch
                {
                    LayoutProperty.Left or LayoutProperty.Top => val => val + offset,
                    LayoutProperty.Right or LayoutProperty.Bottom => val => val - offset,
                    _ => static val => val,
                },
                OffsetType.Outside => property switch
                {
                    LayoutProperty.Left or LayoutProperty.Top => val => val - offset,
                    LayoutProperty.Right or LayoutProperty.Bottom => val => val + offset,
                    _ => static val => val,
                },
                OffsetType.None => static val => val,
                _ => throw new ArgumentOutOfRangeException(nameof(offsetType)),
            };
        }

        public UIElement? DependElement => _dependElementRef.TryGetTarget(out UIElement? result) ? result : null;

        public LayoutProperty DependProperty => _dependProperty;

        public override AbstractCalculation Clone()
            => new ElementDependedCalculation(_dependElementRef, _dependProperty, _func);

        public override ICalculationContext? CreateContext()
            => CalculationContext.TryCreate(_dependElementRef, _dependProperty, _func);

        private sealed class CalculationContext : ICalculationContext
        {
            private readonly UIElement _dependElement;
            private readonly LayoutProperty _dependProperty;
            private readonly Func<int, int> _func;
            private bool _calculated;
            private int _value;

            public bool DependPageRect => false;

            public UIElement DependedElement => _dependElement;

            public LayoutProperty DependedProperty => _dependProperty;

            private CalculationContext(UIElement depend, LayoutProperty property, Func<int, int> func)
            {
                _dependElement = depend;
                _dependProperty = property;
                _func = func;
                _calculated = false;
                _value = 0;
            }

            public static CalculationContext? TryCreate(WeakReference<UIElement> dependRef, LayoutProperty property, Func<int, int> func)
            {
                if (!dependRef.TryGetTarget(out UIElement? depend))
                    return null;
                return new CalculationContext(depend, property, func);
            }

            public bool TryGetResultIfCalculated(out int value)
            {
                if (_calculated)
                {
                    value = _value;
                    return true;
                }
                value = 0;
                return false;
            }

            public unsafe int DoCalc(Rect* pPageRect, int dependedValue)
            {
                if (_calculated)
                    return _value;
                int value = _func(dependedValue);
                _value = value;
                _calculated = true;
                return value;
            }
        }
    }
}
