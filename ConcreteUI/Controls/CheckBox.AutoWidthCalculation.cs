using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private sealed class AutoWidthCalculation : AbstractCalculation
        {
            private readonly WeakReference<CheckBox> _reference;
            private readonly int _minWidth;
            private readonly int _maxWidth;

            public int MinWidth => _minWidth;
            public int MaxWidth => _maxWidth;

            public AutoWidthCalculation(WeakReference<CheckBox> reference, int minWidth = 0, int maxWidth = int.MaxValue)
            {
                _reference = reference;
                _minWidth = minWidth;
                _maxWidth = maxWidth;
            }

            public AutoWidthCalculation(CheckBox element, int minWidth = 0, int maxWidth = int.MaxValue) : this(new WeakReference<CheckBox>(element), minWidth, maxWidth)
            {
            }

            public override AbstractCalculation Clone()
                => new AutoWidthCalculation(_reference, _minWidth, _maxWidth);

            public override ICalculationContext? CreateContext()
                => CalculationContext.TryCreate(_reference, _minWidth, _maxWidth);

            private sealed class CalculationContext : ICalculationContext
            {
                private readonly CheckBox _element;
                private readonly int _minWidth;
                private readonly int _maxWidth;

                private bool _calculated;
                private int _value;

                public bool DependPageRect => false;

                public UIElement DependedElement => _element;

                public LayoutProperty DependedProperty => LayoutProperty.Height;

                private CalculationContext(CheckBox element, int minWidth, int maxWidth)
                {
                    _element = element;
                    _minWidth = minWidth;
                    _maxWidth = maxWidth;
                    _calculated = false;
                    _value = 0;
                }

                public static CalculationContext? TryCreate(WeakReference<CheckBox> reference, int minWidth, int maxWidth)
                {
                    if (!reference.TryGetTarget(out CheckBox? element))
                        return null;
                    return new CalculationContext(element, minWidth, maxWidth);
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
                    int value = MathHelper.Clamp(DoCalcCore(_element) + dependedValue + UIConstants.ElementMargin, _minWidth, _maxWidth);
                    _value = value;
                    _calculated = true;
                    return value;
                }

                private static int DoCalcCore(CheckBox element)
                {
                    using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize);
                    return GraphicsUtils.MeasureTextWidthAsInt(element._text, format);
                }
            }
        }
    }
}
