using System;

using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Layout;

namespace ConcreteUI.Controls
{
    partial class ComboBoxDropdownList
    {
        private sealed class DefaultTopNode : UIElementDependedNode<ComboBoxDropdownList>
        {
            public DefaultTopNode(ComboBoxDropdownList element) : base(element) { }

            protected override int Compute(ComboBoxDropdownList element, in LayoutNodeManager manager)
            {
                int val = manager.GetComputedValue(element._owner, LayoutProperty.Bottom);
                return val - MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(element.Window.PixelsPerPoint.Y));
            }
        }
    }
}
