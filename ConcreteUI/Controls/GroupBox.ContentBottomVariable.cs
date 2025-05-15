using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentBottomVariable : LayoutVariable
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentBottomVariable(GroupBox element)
            {
                _reference = new WeakReference<GroupBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                return GetContentBottomCore(manager.GetComputedValue(element, LayoutProperty.Bottom));
            }
        }
    }
}
