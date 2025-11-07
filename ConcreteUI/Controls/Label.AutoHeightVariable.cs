using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        private sealed class AutoHeightVariable : LayoutVariable
        {
            private readonly WeakReference<Label> _reference;

            public AutoHeightVariable(Label element)
            {
                _reference = new WeakReference<Label>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out Label? element))
                    return 0;
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                float fontSize = element._fontSize;
                string text = element._text;
                if (text.Contains('\n') && manager.GetVariable(element, LayoutProperty.Width) is AutoWidthVariable autoVariable)
                {
                    using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                    layout.MaxWidth = manager.GetComputedValue(autoVariable);
                    return MathI.Ceiling(layout.GetMetrics().Height);
                }
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, fontSize));
            }
        }
    }
}
