using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class ListBox
    {
        private sealed class AutoWidthNode : UIElementDependedNode<ListBox>
        {
            public AutoWidthNode(ListBox element) : base(element) { }

            protected override int Compute(ListBox element, in LayoutNodeManager manager)
            {
                int result = element.GetPredictedWidth();
                if (element.Mode == ListBoxMode.None)
                    return result;
                return result + element.ItemHeight + UIConstants.ElementMargin;
            }
        }
    }
}
