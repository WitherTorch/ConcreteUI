using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ListBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<ListBox>
    {
        public AutoHeightNode(ListBox element) : base(element) { }

        protected override int ComputeCore(ListBox element, in LayoutContext context) => element.GetPredictedHeight();
    }
}
