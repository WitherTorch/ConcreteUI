using ConcreteUI.Layout;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultHeightNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultHeightNode(ComboBoxDropdownList element) : base(element) { }

        protected override int Compute(ComboBoxDropdownList element, in LayoutNodeManager manager)
            => InterlockedHelper.Read(ref element._maxViewHeight);
    }
}
