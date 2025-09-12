using System;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Controls;
using ConcreteUI.Layout.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

#pragma warning disable CS8500

namespace ConcreteUI.Layout
{
    partial class LayoutVariable
    {
        private static readonly LazyTiny<LayoutVariable>[] _smallValueVariableCaches = CreateSmallValueVariableCaches();
        private static readonly LazyTiny<LayoutVariable>[] _pageRectVariableCaches = CreatePageRectVariableCaches();

        private static LazyTiny<LayoutVariable>[] CreateSmallValueVariableCaches()
        {
            LazyTiny<LayoutVariable>[] result = new LazyTiny<LayoutVariable>[256];
            for (int i = 0; i < 128; i++)
            {
                int val = i;
                sbyte value = (sbyte)(byte)val;
                if (value >= 0)
                    val = value + 1;
                result[i] = new LazyTiny<LayoutVariable>(() => new FixedLayoutVariable(val), LazyThreadSafetyMode.PublicationOnly);
            }
            return result;
        }

        private static LazyTiny<LayoutVariable>[] CreatePageRectVariableCaches()
        {
            LazyTiny<LayoutVariable>[] result = new LazyTiny<LayoutVariable>[(int)LayoutProperty._Last];
            for (int i = 0; i < (int)LayoutProperty._Last; i++)
            {
                LayoutProperty prop = (LayoutProperty)i;
                result[i] = new LazyTiny<LayoutVariable>(() => new PageRectLayoutVariable(prop), LazyThreadSafetyMode.PublicationOnly);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe LayoutVariable Fixed(int value)
        {
            if (value == 0)
                return _empty;
            if (value > 0)
            {
                if (value > 128)
                    return new FixedLayoutVariable(value);
                value--;
            }
            else
            {
                if (value < -128)
                    return new FixedLayoutVariable(value);
            }
            if (value < -128 || value > 128)
                return new FixedLayoutVariable(value);
            byte index = (byte)(sbyte)value;
            ref LazyTiny<LayoutVariable> variableCacheRef = ref _smallValueVariableCaches[0];
            return UnsafeHelper.AddTypedOffset(ref variableCacheRef, index).Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable ElementReference(UIElement element, LayoutProperty property)
            => element.GetLayoutReference(property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable PageReference(LayoutProperty property)
        {
            if (property <= LayoutProperty.None || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            ref LazyTiny<LayoutVariable> variableCacheRef = ref _pageRectVariableCaches[0];
            return UnsafeHelper.AddTypedOffset(ref variableCacheRef, (uint)property).Value;
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
