using System;

using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private sealed class ContentHeightVariable : LayoutVariable
        {
            private readonly WeakReference<GroupBox> _reference;

            public ContentHeightVariable(WeakReference<GroupBox> reference) => _reference = reference;

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out GroupBox? element))
                    return 0;
                int top = element.GetContentTopCore(manager.GetComputedValue(element, LayoutProperty.Top));
                int bottom = GetContentBottomCore(manager.GetComputedValue(element, LayoutProperty.Bottom));
                return bottom - top;
            }
        }
    }
}
