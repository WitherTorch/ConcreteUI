using System;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class Button
{
    private sealed class AutoHeightNode : UIElementDependedNode<Button>
    {
        public AutoHeightNode(Button element) : base(element) { }

        protected override int ComputeCOre(Button element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
