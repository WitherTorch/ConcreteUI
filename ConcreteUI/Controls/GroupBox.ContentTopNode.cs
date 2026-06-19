using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls;

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
            return element.GetContentTopCore(manager.GetComputedValue(element, LayoutProperty.Top));
        }
    }
}
