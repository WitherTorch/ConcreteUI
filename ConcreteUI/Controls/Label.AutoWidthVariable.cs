using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        private sealed class AutoWidthVariable : UIElementDependedVariable<Label>
        {
            public AutoWidthVariable(Label element) : base(element) { }

            protected override int Compute(Label element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text,
                    fontName, element._alignment, element._fontSize);
                return MathI.Ceiling(layout.GetMetrics().Width);
            }
        }
    }
}
