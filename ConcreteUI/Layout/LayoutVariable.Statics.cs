using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using InlineMethod;

using WitherTorch.Common.Helpers;

#pragma warning disable CS8500

namespace ConcreteUI.Layout
{
    partial class LayoutVariable
    {
        private const int FixedValueCacheLimit = 256;

        private static readonly FixedLayoutVariable[] _smallValuePositiveVariables = CreateSmallValueVariables_Positive();
        private static readonly FixedLayoutVariable[] _smallValueNegativeVariables = CreateSmallValueVariables_Negative();
        private static readonly PageRectLayoutVariable[] _pageRectVariables = CreatePageRectVariables();

        private static FixedLayoutVariable[] CreateSmallValueVariables_Positive()
        {
            FixedLayoutVariable[] result = new FixedLayoutVariable[FixedValueCacheLimit];
            ref FixedLayoutVariable resultRef = ref UnsafeHelper.GetArrayDataReference(result);
            for (int i = 1; i <= 256; i++)
                UnsafeHelper.AddTypedOffset(ref resultRef, i) = new FixedLayoutVariable(i);
            return result;
        }

        private static FixedLayoutVariable[] CreateSmallValueVariables_Negative()
        {
            FixedLayoutVariable[] result = new FixedLayoutVariable[FixedValueCacheLimit];
            ref FixedLayoutVariable resultRef = ref UnsafeHelper.GetArrayDataReference(result);
            for (int i = 1; i <= 256; i++)
                UnsafeHelper.AddTypedOffset(ref resultRef, i) = new FixedLayoutVariable(-i);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private static PageRectLayoutVariable[] CreatePageRectVariables()
            => new PageRectLayoutVariable[(int)LayoutProperty._Last]
            {
                new PageRectLayoutVariable(LayoutProperty.Left),
                new PageRectLayoutVariable(LayoutProperty.Top),
                new PageRectLayoutVariable(LayoutProperty.Right),
                new PageRectLayoutVariable(LayoutProperty.Bottom),
                new PageRectLayoutVariable(LayoutProperty.Width),
                new PageRectLayoutVariable(LayoutProperty.Height),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable Fixed(int value)
        {
            if (value == 0)
                return _empty;
            if (value < 0)
            {
                int absValue = -value;
                if (absValue < FixedValueCacheLimit)
                    return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_smallValueNegativeVariables), (nuint)absValue);
            }
            else
            {
                if (value < FixedValueCacheLimit)
                    return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_smallValuePositiveVariables), (nuint)value);
            }
            return new FixedLayoutVariable(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable ElementReference(UIElement element, LayoutProperty property)
            => element.GetLayoutReference(property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable PageReference(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_pageRectVariables), (nuint)property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable Custom(Func<LayoutVariableManager, int> computeFunc) => new CustomLayoutVariable(computeFunc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable Max(LayoutVariable left, LayoutVariable right)
        {
            if (ReferenceEquals(left, right))
                return left;
            if (left is FixedLayoutVariable leftFixed && right is FixedLayoutVariable rightFixed)
                return leftFixed.Value > rightFixed.Value ? left : right;
            return new MaxLayoutVariable(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable Min(LayoutVariable left, LayoutVariable right)
        {
            if (ReferenceEquals(left, right))
                return left;
            if (left is FixedLayoutVariable leftFixed && right is FixedLayoutVariable rightFixed)
                return leftFixed.Value < rightFixed.Value ? left : right;
            return new MinLayoutVariable(left, right);
        }
    }
}
