using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentTopNode : UIElementReferencedNode<GroupBox>
    {
        public ContentTopNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context) 
            => element.GetContentTopCore();
    }
}
