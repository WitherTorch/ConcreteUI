using ShioUI.Layout;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class ScrollableElementBase
{
    private sealed class AutoHeightNode : UIElementDependedNode<ScrollableElementBase>
    {
        public AutoHeightNode(ScrollableElementBase element) : base(element) { }

        protected override int Compute(ScrollableElementBase element, in LayoutNodeManager manager)
            => MathHelper.Max(element.SurfaceSize.Height, 1);
    }
}
