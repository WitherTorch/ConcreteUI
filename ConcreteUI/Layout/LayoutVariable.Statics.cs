using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

namespace ConcreteUI.Layout
{
    partial class LayoutVariable
    {
        private static readonly FixedLayoutVariable?[] _smallValueVariableCache = new FixedLayoutVariable?[256];
        private static readonly PageRectLayoutVariable?[] _pageRectVariableCache = new PageRectLayoutVariable?[(int)LayoutProperty._Last];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable Fixed(int value)
        {
            if (value == 0)
                return _empty;
            if (value < -128 || value > 128)
                return new FixedLayoutVariable(value);
            int index;
            if (value < 0)
                index = value + 128;
            else
                index = value + 127;
            return _smallValueVariableCache[index] ??= new FixedLayoutVariable(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable ElementReference(UIElement element, LayoutProperty property)
            => element.GetLayoutReference(property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable PageReference(LayoutProperty property)
            => _pageRectVariableCache[(int)property] ??= new PageRectLayoutVariable(property);

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
