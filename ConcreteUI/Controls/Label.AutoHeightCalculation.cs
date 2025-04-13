using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        private sealed class AutoHeightCalculation : AbstractCalculation
        {
            private readonly WeakReference<Label> _reference;
            private readonly int _minHeight;
            private readonly int _maxHeight;

            public int MinHeight => _minHeight;
            public int MaxHeight => _maxHeight;

            public AutoHeightCalculation(WeakReference<Label> reference, int minHeight = 0, int maxHeight = int.MaxValue)
            {
                _reference = reference;
                _minHeight = minHeight;
                _maxHeight = maxHeight;
            }

            public AutoHeightCalculation(Label element, int minHeight = 0, int maxHeight = int.MaxValue) : this(new WeakReference<Label>(element), minHeight, maxHeight)
            {
            }

            public override AbstractCalculation Clone()
                => new AutoHeightCalculation(_reference, _minHeight, _maxHeight);

            public override ICalculationContext? CreateContext()
                => CalculationContext.TryCreate(_reference, _minHeight, _maxHeight);

            private sealed class CalculationContext : ICalculationContext
            {
                private readonly Label _element;
                private readonly int _minHeight;
                private readonly int _maxHeight;

                private bool _calculated;
                private int _value;

                public bool DependPageRect => false;

                public UIElement? DependedElement => null;

                public LayoutProperty DependedProperty => LayoutProperty.None;

                private CalculationContext(Label element, int minHeight, int maxHeight)
                {
                    _element = element;
                    _minHeight = minHeight;
                    _maxHeight = maxHeight;
                    _calculated = false;
                    _value = 0;
                }

                public static CalculationContext? TryCreate(WeakReference<Label> reference, int minHeight, int maxHeight)
                {
                    if (!reference.TryGetTarget(out Label? element))
                        return null;
                    return new CalculationContext(element, minHeight, maxHeight);
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
                    int value = MathHelper.Clamp(DoCalcCore(_element), _minHeight, _maxHeight);
                    _value = value;
                    _calculated = true;
                    return value;
                }

                [Inline(InlineBehavior.Remove)]
                private static int DoCalcCore(Label element)
                    => MathI.Ceiling(FontHeightHelper.GetFontHeight(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize));
            }
        }
    }
}
