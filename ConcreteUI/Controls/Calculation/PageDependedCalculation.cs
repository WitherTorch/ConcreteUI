using System;

using ConcreteUI.Internals;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class PageDependedCalculation : AbstractCalculation
    {
        private readonly Func<Rect, int> _func;

        public PageDependedCalculation(LayoutProperty property, int margin = UIConstants.ElementMargin)
        {
            switch (property)
            {
                case LayoutProperty.Left:
                    _func = (rect) => rect.Left + margin;
                    break;
                case LayoutProperty.Top:
                    _func = (rect) => rect.Top + margin;
                    break;
                case LayoutProperty.Right:
                    _func = (rect) => rect.Right - margin;
                    break;
                case LayoutProperty.Bottom:
                    _func = (rect) => rect.Bottom - margin;
                    break;
                case LayoutProperty.Height:
                    _func = (rect) => rect.Height;
                    break;
                case LayoutProperty.Width:
                    _func = (rect) => rect.Width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property));
            }
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
