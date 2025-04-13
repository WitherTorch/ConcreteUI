using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        public sealed class AutoWidthCalculation : AbstractCalculation
        {
            private readonly WeakReference<Label> _dependRef;
            private readonly int _minWidth;
            private readonly int _maxWidth;

            public int MinWidth => _minWidth;
            public int MaxWidth => _maxWidth;

            public AutoWidthCalculation(WeakReference<Label> labelRef, int minWidth = 0, int maxWidth = int.MaxValue)
            {
                _dependRef = labelRef;
                _minWidth = minWidth;
                _maxWidth = maxWidth;
            }

            public AutoWidthCalculation(Label label, int minWidth = 0, int maxWidth = int.MaxValue) : this(new WeakReference<Label>(label), minWidth, maxWidth)
            {
            }

            public UIElement? DependedElement => _dependRef.TryGetTarget(out Label? result) ? result : null;

            public LayoutProperty DependedProperty => LayoutProperty.Height;

            public override AbstractCalculation Clone()
                => new AutoWidthCalculation(_dependRef, _minWidth, _maxWidth);

            public override ICalculationContext? CreateContext()
                => CalculationContext.TryCreate(_dependRef, _minWidth, _maxWidth);

            private sealed class CalculationContext : ICalculationContext
            {
                private readonly Label _dependElement;
                private readonly int _minWidth;
                private readonly int _maxWidth;

                private bool _calculated;
                private int _value;

                public bool DependPageRect => false;

                public UIElement DependedElement => _dependElement;

                public LayoutProperty DependedProperty => LayoutProperty.Height;

                private CalculationContext(Label depend, int minWidth, int maxWidth)
                {
                    _dependElement = depend;
                    _minWidth = minWidth;
                    _maxWidth = maxWidth;
                    _calculated = false;
                    _value = 0;
                }

                public static CalculationContext? TryCreate(WeakReference<Label> dependRef, int minWidth, int maxWidth)
                {
                    if (!dependRef.TryGetTarget(out Label? depend))
                        return null;
                    return new CalculationContext(depend, minWidth, maxWidth);
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
                    int value = DoCalc(_dependElement, dependedValue);
                    _value = value;
                    _calculated = true;
                    return value;
                }

                private int DoCalc(Label label, int dependedValue)
                {
                    string? text = label._text;
                    if (StringHelper.IsNullOrEmpty(text))
                        return MathHelper.Max(_minWidth, 0);
                    DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(text, NullSafetyHelper.ThrowIfNull(label._fontName), label._alignment, label._fontSize);
                    if (layout is null)
                        return MathHelper.Max(_minWidth, 0);
                    layout.MaxHeight = dependedValue;
                    int result = MathI.Ceiling(layout.GetMetrics().Width);
                    layout.Dispose();
                    int maxHeight = _maxWidth;
                    if (maxHeight < 0)
                        return result;
                    return MathHelper.Min(result, maxHeight);
                }
            }
        }
    }
}
