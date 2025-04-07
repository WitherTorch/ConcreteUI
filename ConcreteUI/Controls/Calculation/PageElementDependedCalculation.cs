using System;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class PageElementDependedCalculation : AbstractCalculation
    {
        private readonly WeakReference<UIElement> _dependElementRef;
        private readonly LayoutProperty _dependProperty;
        private readonly Func<Rect, int, int> _func;

        private PageElementDependedCalculation(WeakReference<UIElement> dependRef, LayoutProperty property, Func<Rect, int, int> func)
        {
            _dependElementRef = dependRef;
            _dependProperty = property;
            _func = func;
        }

        public PageElementDependedCalculation(UIElement depend, LayoutProperty property, Func<Rect, int, int> func)
            : this(new WeakReference<UIElement>(depend), property, func) { }

        public UIElement DependElement => _dependElementRef.TryGetTarget(out UIElement result) ? result : null;

        public LayoutProperty DependProperty => _dependProperty;

        public override AbstractCalculation Clone()
            => new PageElementDependedCalculation(_dependElementRef, _dependProperty, _func);

        public override ICalculationContext CreateContext()
            => CalculationContext.TryCreate(_dependElementRef, _dependProperty, _func);

        private sealed class CalculationContext : ICalculationContext
        {
            private readonly UIElement _dependElement;
            private readonly LayoutProperty _dependProperty;
            private readonly Func<Rect, int, int> _func;
            private bool _calculated;
            private int _value;

            public bool DependPageRect => true;

            public UIElement DependedElement => _dependElement;

            public LayoutProperty DependedProperty => _dependProperty;

            private CalculationContext(UIElement depend, LayoutProperty property, Func<Rect, int, int> func)
            {
                _dependElement = depend;
                _dependProperty = property;
                _func = func;
                _calculated = false;
                _value = 0;
            }

            public static CalculationContext TryCreate(WeakReference<UIElement> dependRef, LayoutProperty property, Func<Rect, int, int> func)
            {
                if (!dependRef.TryGetTarget(out UIElement depend))
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
                int value = _func(*pPageRect, dependedValue);
                _value = value;
                _calculated = true;
                return value;
            }
        }

    }
}
