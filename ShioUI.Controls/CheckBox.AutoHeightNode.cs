using System;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class CheckBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<CheckBox>
    {
        public AutoHeightNode(CheckBox element) : base(element) { }

        protected override int ComputeCore(CheckBox element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
