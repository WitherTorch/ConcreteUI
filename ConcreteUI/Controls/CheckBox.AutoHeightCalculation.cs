using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        public sealed class AutoHeightCalculation : AbstractCalculation
        {
            private readonly WeakReference<CheckBox> _reference;
            private readonly int _minHeight;
            private readonly int _maxHeight;

            public AutoHeightCalculation(WeakReference<CheckBox> reference, int minHeight = -1, int maxHeight = -1)
            {
                _reference = reference;
                _minHeight = minHeight;
                _maxHeight = maxHeight;
            }

            public AutoHeightCalculation(CheckBox element, int minHeight = -1, int maxHeight = -1) : this(new WeakReference<CheckBox>(element), minHeight, maxHeight)
            {
            }

            public override AbstractCalculation Clone()
                => new AutoHeightCalculation(_reference, _minHeight, _maxHeight);

            public override ICalculationContext CreateContext()
                => CalculationContext.TryCreate(_reference, _minHeight, _maxHeight);

            private sealed class CalculationContext : ICalculationContext
            {
                private readonly CheckBox _element;
                private readonly int _minHeight;
                private readonly int _maxHeight;

                private bool _calculated;
                private int _value;

                public bool DependPageRect => false;

                public UIElement DependedElement => null;

                public LayoutProperty DependedProperty => LayoutProperty.None;

                private CalculationContext(CheckBox element, int minHeight, int maxHeight)
                {
                    _element = element;
                    _minHeight = minHeight;
                    _maxHeight = maxHeight;
                    _calculated = false;
                    _value = 0;
                }

                public static CalculationContext TryCreate(WeakReference<CheckBox> reference, int minHeight, int maxHeight)
                {
                    if (!reference.TryGetTarget(out CheckBox element))
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
                    int value = DoCalc(_element) + UIConstants.ElementMargin;
                    _value = value; 
                    _calculated = true;
                    return value;
                }

                private int DoCalc(CheckBox element)
                {
                    string text = element._text;
                    if (string.IsNullOrEmpty(text))
                        return MathHelper.Max(_minHeight, 0);
                    DWriteTextLayout layout = TextFormatUtils.CreateTextLayout(text, element._fontName, TextAlignment.MiddleLeft, element._fontSize);
                    if (layout is null)
                        return MathHelper.Max(_minHeight, 0);
                    int result = MathI.Ceiling(layout.GetMetrics().Height);
                    layout.Dispose();
                    int maxHeight = _maxHeight;
                    if (maxHeight < 0)
                        return result;
                    return MathHelper.Min(result, maxHeight);
                }
            }
        }
    }
}
