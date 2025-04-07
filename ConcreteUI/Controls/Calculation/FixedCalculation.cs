using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public sealed class FixedCalculation : AbstractCalculation
    {
        private readonly int value;

        public FixedCalculation(int value)
        {
            this.value = value;
        }

        public override AbstractCalculation Clone() => new FixedCalculation(value);

        public override ICalculationContext CreateContext() => new CalculationContext(value);

        private sealed class CalculationContext : ICalculationContext
        {
            private readonly int _value;

            public bool DependPageRect => false;

            public UIElement DependedElement => null;

            public LayoutProperty DependedProperty => LayoutProperty.None;

            public CalculationContext(int value)
            {
                _value = value;
            }

            public bool TryGetResultIfCalculated(out int value)
            {
                value = _value;
                return true;
            }

            public unsafe int DoCalc(Rect* pPageRect, int dependedValue)
                => _value;
        }
    }
}
