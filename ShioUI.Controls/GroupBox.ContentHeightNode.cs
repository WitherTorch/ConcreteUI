using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentHeightNode : UIElementReferencedNode<GroupBox>
    {
        public ContentHeightNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context)
            => element.GetContentHeightCore(context.GetComputedValue(element, LayoutProperty.Height));
    }
}
