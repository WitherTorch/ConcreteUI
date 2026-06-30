using System;

using ShioUI.Controls.Internals;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class Label
{
    private sealed class AutoHeightNode : UIElementDependedNode<Label>
    {
        public AutoHeightNode(Label element) : base(element) { }

        protected override int ComputeCore(Label element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            float fontSize = element._fontSize;
            string text = element._text;
            if (SequenceHelper.Contains(text, '\n') && context.GetLayoutNodeOrNull(element, LayoutProperty.Width) is AutoWidthNode autoVariable)
            {
                using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                layout.MaxWidth = context.GetComputedValue(autoVariable);
                return MathI.Ceiling(layout.GetMetrics().Height);
            }
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, fontSize));
        }
    }
}
