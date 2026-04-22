using System;

using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class ComboBoxDropdownList
    {
        private sealed class DefaultTopVariable : UIElementDependedVariable<ComboBoxDropdownList>
        {
            public DefaultTopVariable(ComboBoxDropdownList element) : base(element) { }

            protected override int Compute(ComboBoxDropdownList element, in LayoutVariableManager manager)
            {
                int val = manager.GetComputedValue(element._owner, LayoutProperty.Bottom);
                return val - MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(element.Window.PixelsPerPoint.Y));
            }
        }
    }
}
