using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        private sealed class AutoHeightVariable : UIElementDependedVariable<Label>
        {
            public AutoHeightVariable(Label element) : base(element) { }

            protected override int Compute(Label element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                float fontSize = element._fontSize;
                string text = element._text;
                if (SequenceHelper.Contains(text, '\n') && manager.GetVariable(element, LayoutProperty.Width) is AutoWidthVariable autoVariable)
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
