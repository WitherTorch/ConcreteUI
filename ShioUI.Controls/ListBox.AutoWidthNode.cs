using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ListBox
{
    private sealed class AutoWidthNode : UIElementDependedNode<ListBox>
    {
        public AutoWidthNode(ListBox element) : base(element) { }

        protected override int ComputeCore(ListBox element, in LayoutContext context)
        {
            int result = element.GetPredictedWidth();
            if (element.Mode == ListBoxMode.None)
                return result;
            return result + element.ItemHeight + UIConstants.ElementMargin;
        }
    }
}
