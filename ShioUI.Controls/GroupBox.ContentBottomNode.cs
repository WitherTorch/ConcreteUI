using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentBottomNode : UIElementReferencedNode<GroupBox>
    {
        public ContentBottomNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context) 
            => GetContentBottomCore(context.GetComputedValue(element, LayoutProperty.Height));
    }
}
