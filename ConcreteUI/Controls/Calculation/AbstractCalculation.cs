using System;

namespace ConcreteUI.Controls.Calculation
{
    public abstract class AbstractCalculation : ICloneable
    {
        public abstract ICalculationContext CreateContext();

        public abstract AbstractCalculation Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
