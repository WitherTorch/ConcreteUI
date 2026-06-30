using System;

using ShioUI.Graphics.Helpers;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultTopNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        private readonly int _baseY;

        public DefaultTopNode(ComboBoxDropdownList element, int baseY) : base(element) => _baseY = baseY;

        protected override int ComputeCOre(ComboBoxDropdownList element, in LayoutContext context) 
            => _baseY - MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(element.Window.GetPixelsPerPoint().Y));
    }
}
