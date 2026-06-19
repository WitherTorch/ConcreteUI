using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls;

partial class CheckBox
{
    private sealed class AutoWidthNode : UIElementDependedNode<CheckBox>
    {
        public AutoWidthNode(CheckBox element) : base(element) { }

        protected override int Compute(CheckBox element, in LayoutNodeManager manager)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, element._fontSize);
            return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + manager.GetComputedValue(element, LayoutProperty.Height) + UIConstants.ElementMargin;
        }
    }
}
