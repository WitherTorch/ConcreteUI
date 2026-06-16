using ConcreteUI.Layout;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls;

partial class ScrollableElementBase
{
    private sealed class AutoHeightNode : UIElementDependedNode<ScrollableElementBase>
    {
        public AutoHeightNode(ScrollableElementBase element) : base(element) { }

        protected override int Compute(ScrollableElementBase element, in LayoutNodeManager manager)
            => MathHelper.Max(element.SurfaceSize.Height, 1);
    }
}
