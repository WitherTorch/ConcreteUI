﻿using System;

using ConcreteUI.Controls.Calculation;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        public sealed class AutoWidthCalculation : AbstractCalculation
        {
            private readonly WeakReference<CheckBox> _reference;
            private readonly int _minWidth;
            private readonly int _maxWidth;

            public AutoWidthCalculation(WeakReference<CheckBox> reference, int minWidth = -1, int maxWidth = -1)
            {
                _reference = reference;
                _minWidth = minWidth;
                _maxWidth = maxWidth;
            }

            public AutoWidthCalculation(CheckBox element, int minWidth = -1, int maxWidth = -1) : this(new WeakReference<CheckBox>(element), minWidth, maxWidth)
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
                    int value = DoCalc(_element) + dependedValue + MathI.Ceiling(_element.Renderer.GetBaseLineWidth()) + 3;
                    _value = value;
                    _calculated = true;
                    return value;
                }

                private int DoCalc(CheckBox element)
                {
                    string? text = element._text;
                    if (StringHelper.IsNullOrEmpty(text))
                        return MathHelper.Max(_minWidth, 0);
                    DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(text, NullSafetyHelper.ThrowIfNull(element._fontName), TextAlignment.MiddleLeft, element._fontSize);
                    if (layout is null)
                        return MathHelper.Max(_minWidth, 0);
                    int result = MathI.Ceiling(layout.GetMetrics().Width);
                    layout.Dispose();
                    int maxWidth = _maxWidth;
                    if (maxWidth < 0)
                        return result;
                    return MathHelper.Min(result, maxWidth);
                }
            }
        }
    }
}
