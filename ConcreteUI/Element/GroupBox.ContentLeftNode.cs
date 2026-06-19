using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Element;

partial class GroupBox
{
    private sealed class ContentLeftNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentLeftNode(WeakReference<GroupBox> reference) => _reference = reference;

        public override int Compute(in LayoutNodeManager manager)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            return GetContentLeftCore(manager.GetComputedValue(element, LayoutProperty.Left));
        }
    }
}
