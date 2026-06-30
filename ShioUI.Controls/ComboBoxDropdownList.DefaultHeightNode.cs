using ShioUI.Layout;

using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultHeightNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultHeightNode(ComboBoxDropdownList element) : base(element) { }

        protected override int ComputeCOre(ComboBoxDropdownList element, in LayoutContext context)
            => InterlockedHelper.Read(ref element._maxViewHeight);
    }
}
