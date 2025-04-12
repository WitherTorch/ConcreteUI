using System;

using ConcreteUI.Internals;

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
            : this(depend, property, AutoConfigureCalculation(property, UIConstants.ElementMargin, MarginType.Inside)) { }
        
        public ElementDependedCalculation(UIElement depend, LayoutProperty property,
            MarginType marginType, int margin = UIConstants.ElementMargin)
            : this(depend, property, AutoConfigureCalculation(property, margin, marginType)) { }

        public ElementDependedCalculation(UIElement depend, LayoutProperty property, Func<int, int> func)
            : this(new WeakReference<UIElement>(depend), property, func) { }

        private static Func<int, int> AutoConfigureCalculation(LayoutProperty property, int margin, MarginType marginType)
        {
            if (margin == 0)
                marginType = MarginType.None;
            switch (marginType)
            {
                case MarginType.Outside:
                    {
                        return property switch
                        {
                            LayoutProperty.Left or LayoutProperty.Top => AutoConfigureCalculation(property, margin, MarginType.Subtract),
                            LayoutProperty.Right or LayoutProperty.Bottom => AutoConfigureCalculation(property, margin, MarginType.Add),
                            LayoutProperty.Height or LayoutProperty.Width => AutoConfigureCalculation(property, margin, MarginType.None),
                            _ => throw new ArgumentException("Invalid Layout Property!", nameof(property)),
                        };
                    }
                case MarginType.Inside:
                    {
                        return property switch
                        {
                            LayoutProperty.Left or LayoutProperty.Top => AutoConfigureCalculation(property, margin, MarginType.Add),
                            LayoutProperty.Right or LayoutProperty.Bottom => AutoConfigureCalculation(property, margin, MarginType.Subtract),
                            LayoutProperty.Height or LayoutProperty.Width => AutoConfigureCalculation(property, margin, MarginType.None),
                            _ => throw new ArgumentException("Invalid Layout Property!", nameof(property)),
                        };
                    }
                case MarginType.None:
                    return static (val) => val;
                case MarginType.Add:
                    return (val) => val + margin;
                case MarginType.Subtract:
                    return (val) => val - margin;
                default:
                    throw new ArgumentException("Invalid Layout Property!", nameof(property));
            }
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
