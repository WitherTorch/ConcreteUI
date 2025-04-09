using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls.Calculation
{
    public unsafe interface ICalculationContext
    {
        bool DependPageRect { get; }

        UIElement? DependedElement { get; }

        LayoutProperty DependedProperty { get; }

        bool TryGetResultIfCalculated(out int value);

        int DoCalc(Rect* pPageRect, int dependedValue);
    }
}
