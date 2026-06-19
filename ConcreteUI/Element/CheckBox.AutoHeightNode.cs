using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Element;

partial class CheckBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<CheckBox>
    {
        public AutoHeightNode(CheckBox element) : base(element) { }

        protected override int Compute(CheckBox element, in LayoutNodeManager manager)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
