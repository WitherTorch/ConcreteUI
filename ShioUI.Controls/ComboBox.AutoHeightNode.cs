using System;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class ComboBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<ComboBox>
    {
        public AutoHeightNode(ComboBox element) : base(element) { }

        protected override int ComputeCore(ComboBox element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
