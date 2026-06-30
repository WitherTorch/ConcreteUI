using System;

using ShioUI.Controls.Internals;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class Label
{
    private sealed class AutoWidthNode : UIElementDependedNode<Label>
    {
        public AutoWidthNode(Label element) : base(element) { }

        protected override int ComputeCore(Label element, in LayoutContext context)
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
