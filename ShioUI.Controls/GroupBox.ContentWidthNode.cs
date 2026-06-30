using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentWidthNode : LayoutNode
    {
        private readonly WeakReference<GroupBox> _reference;

        public ContentWidthNode(WeakReference<GroupBox> reference) => _reference = reference;

        protected override int ComputeCore(in LayoutContext context)
        {
            if (!_reference.TryGetTarget(out GroupBox? element))
                return 0;
            return GetContentWidthCore(context.GetComputedValue(element, LayoutProperty.Width));
        }
    }
}
