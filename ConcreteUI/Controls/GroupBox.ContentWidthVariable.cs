using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentWidthVariable : LayoutVariable
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentWidthVariable(GroupBox element)
            {
                _reference = new WeakReference<GroupBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                int left = GetContentLeftCore(manager.GetComputedValue(element, LayoutProperty.Left));
                int right = GetContentRightCore(manager.GetComputedValue(element, LayoutProperty.Right));
                return right - left;
            }
        }
    }
}
