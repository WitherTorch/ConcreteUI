using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        private sealed class AutoHeightVariable : UIElementDependedVariable<TextBox>
        {
            public AutoHeightVariable(TextBox element) : base(element) { }

            protected override int Compute(TextBox element, in LayoutVariableManager manager) 
                => MathI.Ceiling(ComputeCore(element, manager)) + UIConstants.ElementMargin;

            private static float ComputeCore(TextBox element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                float fontSize = element._fontSize;
                if (element._multiLine)
                {
                    using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                    element.SetRenderingPropertiesForMultiLine(layout, manager.GetComputedValue(element, LayoutProperty.Width) - UIConstants.ElementMargin,
                        element.Renderer.GetPixelsPerPoint());
                    return layout.GetMetrics().Height;
                }
                return FontHeightHelper.GetFontHeight(fontName, fontSize);
            }
        }
    }
}
