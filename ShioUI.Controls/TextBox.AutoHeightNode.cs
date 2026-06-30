using System;

using ShioUI.Controls.Internals;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class TextBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<TextBox>
    {
        public AutoHeightNode(TextBox element) : base(element) { }

        protected override int ComputeCore(TextBox element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return UIConstants.ElementMargin;
            float result, fontSize = element._fontSize;
            if (element._multiLine)
            {
                using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                element.SetRenderingPropertiesForMultiLine(layout, context.GetComputedValue(element, LayoutProperty.Width) - UIConstants.ElementMargin,
                    element.Window.GetPixelsPerPoint());
                result = layout.GetMetrics().Height;
            }
            else
                result = FontHeightHelper.GetFontHeight(fontName, fontSize);
            return MathI.Ceiling(result) + UIConstants.ElementMargin;
        }
    }
}
