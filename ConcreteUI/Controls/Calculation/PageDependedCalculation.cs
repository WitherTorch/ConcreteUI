using System;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class PageDependedCalculation : AbstractCalculation
    {
        private readonly Func<Rect, int> _func;

        public PageDependedCalculation(LayoutProperty property, int margin = UIConstants.ElementMargin)
        {
            _func = property switch
            {
                LayoutProperty.Left => (rect) => rect.Left + margin,
                LayoutProperty.Top => (rect) => rect.Top + margin,
                LayoutProperty.Right => (rect) => rect.Right - margin,
                LayoutProperty.Bottom => (rect) => rect.Bottom - margin,
                LayoutProperty.Height => (rect) => rect.Height,
                LayoutProperty.Width => (rect) => rect.Width,
                _ => throw new ArgumentOutOfRangeException(nameof(property)),
            };
        }

        public PageDependedCalculation(Func<Rect, int> func)
        {
            _func = func;
        }

        public override AbstractCalculation Clone()
            => new PageDependedCalculation(_func);

        public override ICalculationContext? CreateContext()
            => new CalculationContext(_func);

        private sealed class CalculationContext : ICalculationContext
        {
            private readonly Func<Rect, int> _func;
            private bool _calculated;
            private int _value;

            public bool DependPageRect => true;

            public UIElement? DependedElement => null;

            public LayoutProperty DependedProperty => LayoutProperty.None;

            public CalculationContext(Func<Rect, int> func)
            {
                _func = func;
                _calculated = false;
                _value = 0;
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
                int value = _func(*pPageRect);
                _value = value;
                _calculated = true;
                return value;
            }
        }
    }
}
