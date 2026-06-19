using System;

using ConcreteUI.Controls.Internals;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;

namespace ConcreteUI.Controls;

partial class Label
{
    private sealed class AutoWidthNode : UIElementDependedNode<Label>
    {
        public AutoWidthNode(Label element) : base(element) { }

        protected override int Compute(Label element, in LayoutNodeManager manager)
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
