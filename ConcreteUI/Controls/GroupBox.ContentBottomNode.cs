using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentBottomNode : LayoutNode
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentBottomNode(WeakReference<GroupBox> reference) => _reference = reference;

            public override int Compute(in LayoutNodeManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                return GetContentBottomCore(manager.GetComputedValue(element, LayoutProperty.Bottom));
            }
        }
    }
}
