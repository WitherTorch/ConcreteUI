using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls;

partial class ComboBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<ComboBox>
    {
        public AutoHeightNode(ComboBox element) : base(element) { }

        protected override int Compute(ComboBox element, in LayoutNodeManager manager)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
