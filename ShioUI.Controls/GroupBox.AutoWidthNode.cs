using System;

using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

#pragma warning disable CS0162

partial class GroupBox
{
    private sealed class AutoWidthNode : UIElementReferencedNode<GroupBox>
    {
        public AutoWidthNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context)
        {
            var computeResult = ContainerAutoSizeHelper.Compute(this, element, context,
                    deltaStart: LayoutProperty.Left, deltaEnd: LayoutProperty.Right, initialValue: context.PageSize.Width);
            if (ContentLeftPadding == ContentRightPadding)
                return computeResult.Result + ContentLeftPadding;
            else
            {
                int mask = (MathHelper.BooleanToInt32(computeResult.ReversedFlow) - 1); // 反相取遮罩
                return computeResult.Result + ((ContentLeftPadding & mask) | (ContentRightPadding & ~mask));
            }
        }
    }
}
