using ConcreteUI.Layout;

namespace ConcreteUI.Element;

partial class ListBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<ListBox>
    {
        public AutoHeightNode(ListBox element) : base(element) { }

        protected override int Compute(ListBox element, in LayoutNodeManager manager) => element.GetPredictedHeight();
    }
}
