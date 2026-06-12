using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls;

partial class GroupBox
{
    private sealed class ContentWidthNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentWidthNode(WeakReference<GroupBox> reference) => _reference = reference;

        public override int Compute(in LayoutNodeManager manager)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            int left = GetContentLeftCore(manager.GetComputedValue(element, LayoutProperty.Left));
            int right = GetContentRightCore(manager.GetComputedValue(element, LayoutProperty.Right));
            return right - left;
        }
    }
}
