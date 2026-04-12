using System;

using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class ComboBoxDropdownList
    {
        private sealed class DefaultTopVariable : LayoutVariable
        {
            private readonly WeakReference<ComboBoxDropdownList> _ownerRef;

            public DefaultTopVariable(ComboBoxDropdownList owner)
            {
                _ownerRef = new WeakReference<ComboBoxDropdownList>(owner);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_ownerRef.TryGetTarget(out ComboBoxDropdownList? owner))
                    return 0;
                int val = manager.GetComputedValue(owner._owner, LayoutProperty.Bottom);
                return val - MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(owner.Window.PixelsPerPoint.Y));
            }
        }
    }
}
