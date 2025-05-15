using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentTopVariable : LayoutVariable
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentTopVariable(GroupBox element)
            {
                _reference = new WeakReference<GroupBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                return element.GetContentTopCore(manager.GetComputedValue(element, LayoutProperty.Top));
            }
        }
    }
}
