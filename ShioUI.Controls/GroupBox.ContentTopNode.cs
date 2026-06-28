using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentTopNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentTopNode(WeakReference<GroupBox> reference) => _reference = reference;

        public override int Compute(in LayoutNodeManager manager)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            return element.GetContentTopCore();
        }
    }
}
