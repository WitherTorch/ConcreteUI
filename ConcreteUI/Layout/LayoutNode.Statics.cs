using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using InlineMethod;

using WitherTorch.Common.Helpers;

#pragma warning disable CS8500

namespace ConcreteUI.Layout
{
    partial class LayoutNode
    {
        private const int FixedValueCacheLimit = 256;

        private static readonly FixedValueLayoutNode[] _smallValuePositiveNodes = CreateSmallValueNodes_Positive();
        private static readonly FixedValueLayoutNode[] _smallValueNegativeNodes = CreateSmallValueNodes_Negative();
        private static readonly PageRectLayoutNode[] _pageRectNodes = CreatePageRectNodes();

        private static FixedValueLayoutNode[] CreateSmallValueNodes_Positive()
        {
            FixedValueLayoutNode[] result = new FixedValueLayoutNode[FixedValueCacheLimit];
            ref FixedValueLayoutNode resultRef = ref UnsafeHelper.GetArrayDataReference(result);
            for (int i = 1; i <= 256; i++)
                UnsafeHelper.AddTypedOffset(ref resultRef, i) = new FixedValueLayoutNode(i);
            return result;
        }

        private static FixedValueLayoutNode[] CreateSmallValueNodes_Negative()
        {
            FixedValueLayoutNode[] result = new FixedValueLayoutNode[FixedValueCacheLimit];
            ref FixedValueLayoutNode resultRef = ref UnsafeHelper.GetArrayDataReference(result);
            for (int i = 1; i <= 256; i++)
                UnsafeHelper.AddTypedOffset(ref resultRef, i) = new FixedValueLayoutNode(-i);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private static PageRectLayoutNode[] CreatePageRectNodes()
            => new PageRectLayoutNode[(int)LayoutProperty._Last]
            {
                new PageRectLayoutNode(LayoutProperty.Left),
                new PageRectLayoutNode(LayoutProperty.Top),
                new PageRectLayoutNode(LayoutProperty.Right),
                new PageRectLayoutNode(LayoutProperty.Bottom),
                new PageRectLayoutNode(LayoutProperty.Width),
                new PageRectLayoutNode(LayoutProperty.Height),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Fixed(int value)
        {
            if (value == 0)
                return _empty;
            if (value < 0)
            {
                int absValue = -value;
                if (absValue < FixedValueCacheLimit)
                    return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_smallValueNegativeNodes), (nuint)absValue);
            }
            else
            {
                if (value < FixedValueCacheLimit)
                    return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_smallValuePositiveNodes), (nuint)value);
            }
            return new FixedValueLayoutNode(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Element(UIElement element, LayoutProperty property)
            => element.GetLayoutReference(property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Page(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_pageRectNodes), (nuint)property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Custom(Func<LayoutNodeManager, int> computeFunc) => new CustomLayoutNode(computeFunc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Max(LayoutNode left, LayoutNode right)
        {
            if (ReferenceEquals(left, right))
                return left;
            if (left is FixedValueLayoutNode leftFixed && right is FixedValueLayoutNode rightFixed)
                return leftFixed.Value > rightFixed.Value ? left : right;
            return new MaxLayoutNode(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Min(LayoutNode left, LayoutNode right)
        {
            if (ReferenceEquals(left, right))
                return left;
            if (left is FixedValueLayoutNode leftFixed && right is FixedValueLayoutNode rightFixed)
                return leftFixed.Value < rightFixed.Value ? left : right;
            return new MinLayoutNode(left, right);
        }
    }
}
