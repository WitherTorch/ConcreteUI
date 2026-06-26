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

        protected override int Compute(Label element, in LayoutNodeManager manager)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            float fontSize = element._fontSize;
            string text = element._text;
            if (SequenceHelper.Contains(text, '\n') && manager.GetLayoutNodeOrNull(element, LayoutProperty.Width) is AutoWidthNode autoVariable)
            {
                using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                layout.MaxWidth = manager.GetComputedValue(autoVariable);
                return MathI.Ceiling(layout.GetMetrics().Height);
            }
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, fontSize));
        }
    }
}
