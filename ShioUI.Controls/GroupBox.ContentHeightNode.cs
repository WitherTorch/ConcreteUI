using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentHeightNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentHeightNode(WeakReference<GroupBox> reference) => _reference = reference;

        protected override int ComputeCore(in LayoutNodeManager manager)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            return element.GetContentHeightCore(manager.GetComputedValue(element, LayoutProperty.Height));
        }
    }
}
