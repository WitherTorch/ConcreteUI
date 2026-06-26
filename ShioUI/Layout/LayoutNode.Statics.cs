using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals;

using RiceTea.Core.Helpers;

#pragma warning disable CS8500

namespace ShioUI.Layout;

public delegate int CustomComputeDelegate(in LayoutNodeManager manager);

partial class LayoutNode
{
    private const int FixedValueCacheLimit = 256;

    private static readonly FixedValueLayoutNode[] _smallValuePositiveNodes = CreateSmallValueNodes_Positive();
    private static readonly FixedValueLayoutNode[] _smallValueNegativeNodes = CreateSmallValueNodes_Negative();

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode Fixed(int value)
    {
        if (value == 0)
            return Empty;
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
        => element.GetLayoutDefinition(property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode Custom(CustomComputeDelegate computeFunc) => new CustomLayoutNode(computeFunc);

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
