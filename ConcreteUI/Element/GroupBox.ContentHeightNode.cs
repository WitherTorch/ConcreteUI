using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Element;

partial class GroupBox
{
    private sealed class ContentHeightNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentHeightNode(WeakReference<GroupBox> reference) => _reference = reference;

        public override int Compute(in LayoutNodeManager manager)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            int top = element.GetContentTopCore(manager.GetComputedValue(element, LayoutProperty.Top));
            int bottom = GetContentBottomCore(manager.GetComputedValue(element, LayoutProperty.Bottom));
            return bottom - top;
        }
    }
}
