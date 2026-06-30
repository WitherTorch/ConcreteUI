using System;

using RiceTea.Core.Helpers;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class AutoHeightNode : UIElementReferencedNode<GroupBox>
    {
        public AutoHeightNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context)
        {
            (int result, bool reversedFlow) = ContainerAutoSizeHelper.Compute(this, element, context,
                    deltaStart: LayoutProperty.Top, deltaEnd: LayoutProperty.Bottom, initialValue: context.PageSize.Height);
            if (reversedFlow)
                return result + element.GetContentTopCore();
            else
                return result + ContentBottomPadding;
        }
    }
}
