﻿using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        public sealed class AutoHeightCalculation : AbstractCalculation
        {
            private readonly WeakReference<Label> _reference;
            private readonly int _minHeight;
            private readonly int _maxHeight;

            public AutoHeightCalculation(WeakReference<Label> reference, int minHeight = -1, int maxHeight = -1)
            {
                _reference = reference;
                _minHeight = minHeight;
                _maxHeight = maxHeight;
            }

            public AutoHeightCalculation(Label element, int minHeight = -1, int maxHeight = -1) : this(new WeakReference<Label>(element), minHeight, maxHeight)
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

                public UIElement DependedElement => _element;

                public LayoutProperty DependedProperty => LayoutProperty.Width;

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
                    int value = DoCalc(_element, dependedValue);
                    _value = value;
                    _calculated = true;
                    return value;
                }

                private int DoCalc(Label element, int dependedValue)
                {
                    string text = element._text ?? string.Empty;
                    DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(text, NullSafetyHelper.ThrowIfNull(element._fontName), element._alignment, element._fontSize);
                    if (layout is null)
                        return MathHelper.Max(_minHeight, 0);
                    layout.MaxWidth = dependedValue;
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
