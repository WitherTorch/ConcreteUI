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

        protected override int Compute(TextBox element, in LayoutNodeManager manager) 
            => MathI.Ceiling(ComputeCore(element, manager)) + UIConstants.ElementMargin;

        private static float ComputeCore(TextBox element, in LayoutNodeManager manager)
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
