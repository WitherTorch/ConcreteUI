using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentRightVariable : LayoutVariable
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentRightVariable(GroupBox element)
            {
                _reference = new WeakReference<GroupBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                return GetContentRightCore(manager.GetComputedValue(element, LayoutProperty.Right));
            }
        }
    }
}
