using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class CheckBox
{
    private sealed class AutoWidthNode : UIElementDependedNode<CheckBox>
    {
        public AutoWidthNode(CheckBox element) : base(element) { }

        protected override int ComputeCore(CheckBox element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, element._fontSize);
            return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + context.GetComputedValue(element, LayoutProperty.Height) + UIConstants.ElementMargin;
        }
    }
}
