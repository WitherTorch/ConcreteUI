using System;

using InlineMethod;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class PageDependedCalculation : AbstractCalculation
    {
        private readonly Func<Rect, int> _func;

        public PageDependedCalculation(LayoutProperty property) : this(AutoConfigureCalculation(property)) { }

        public PageDependedCalculation(LayoutProperty property, int offset) : this(AutoConfigureCalculation(property, offset)) { }

        public PageDependedCalculation(Func<Rect, int> func)
        {
            _func = func;
        }

        [Inline(InlineBehavior.Remove)]
        private static Func<Rect, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property)
            => property switch
            {
                LayoutProperty.Left => static val => val.Left + UIConstants.ElementMargin,
                LayoutProperty.Top => static val => val.Top + UIConstants.ElementMargin,
                LayoutProperty.Right => static val => val.Right - UIConstants.ElementMargin,
                LayoutProperty.Bottom => static val => val.Bottom - UIConstants.ElementMargin,
                LayoutProperty.Height => static val => val.Height,
                LayoutProperty.Width => static val => val.Width,
                _ => throw new ArgumentOutOfRangeException(nameof(property)),
            };

        [Inline(InlineBehavior.Remove)]
        private static Func<Rect, int> AutoConfigureCalculation([InlineParameter] LayoutProperty property, [InlineParameter] int offset)
        {
            if (offset == 0)
            {
                return property switch
                {
                    LayoutProperty.Left => static val => val.Left,
                    LayoutProperty.Top => static val => val.Top,
                    LayoutProperty.Right => static val => val.Right,
                    LayoutProperty.Bottom => static val => val.Bottom,
                    LayoutProperty.Height => static val => val.Height,
                    LayoutProperty.Width => static val => val.Width,
                    _ => throw new ArgumentOutOfRangeException(nameof(property)),
                };
            }
            if (offset == UIConstants.ElementMargin)
            {
                return property switch
                {
                    LayoutProperty.Left => static val => val.Left + UIConstants.ElementMargin,
                    LayoutProperty.Top => static val => val.Top + UIConstants.ElementMargin,
                    LayoutProperty.Right => static val => val.Right - UIConstants.ElementMargin,
                    LayoutProperty.Bottom => static val => val.Bottom - UIConstants.ElementMargin,
                    LayoutProperty.Height => static val => val.Height - UIConstants.ElementMargin,
                    LayoutProperty.Width => static val => val.Width - UIConstants.ElementMargin,
                    _ => throw new ArgumentOutOfRangeException(nameof(property)),
                };
            }
            return property switch
            {
                LayoutProperty.Left => val => val.Left + offset,
                LayoutProperty.Top => val => val.Top + offset,
                LayoutProperty.Right => val => val.Right - offset,
                LayoutProperty.Bottom => val => val.Bottom - offset,
                LayoutProperty.Height => val => val.Height - offset,
                LayoutProperty.Width => val => val.Width - offset,
                _ => throw new ArgumentOutOfRangeException(nameof(property)),
            };
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
