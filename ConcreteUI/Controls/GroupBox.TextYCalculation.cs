using System;

using ConcreteUI.Controls.Calculation;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        public sealed class ContentYCalculation : AbstractCalculation
        {
            private readonly WeakReference<GroupBox> _reference;
            private readonly int _offset;

            public ContentYCalculation(WeakReference<GroupBox> reference, int offset = 0)
            {
                _reference = reference;
                _offset = offset;
            }

            public ContentYCalculation(GroupBox element, int offset = 0) : this(new WeakReference<GroupBox>(element), offset)
            {
            }

            public override AbstractCalculation Clone() => new ContentXCalculation(_reference);

            public override ICalculationContext? CreateContext() => CalculationContext.TryCreate(_reference, _offset);

            private sealed class CalculationContext : ICalculationContext
            {
                private readonly GroupBox _element; 
                private readonly int _offset;

                private bool _calculated;
                private int _value;

                public bool DependPageRect => false;

                public UIElement DependedElement => _element;

                public LayoutProperty DependedProperty => LayoutProperty.Top;

                private CalculationContext(GroupBox element, int offset)
                {
                    _element = element;
                    _offset = offset;
                    _calculated = false;
                    _value = 0;
                }

                public static CalculationContext? TryCreate(WeakReference<GroupBox> reference, int offset)
                {
                    if (!reference.TryGetTarget(out GroupBox? element))
                        return null;
                    return new CalculationContext(element, offset);
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
                    int value = _element.GetContentBaseYCore(dependedValue) + _offset;
                    _value = value;
                    _calculated = true;
                    return value;
                }
            }
        }
    }
}
