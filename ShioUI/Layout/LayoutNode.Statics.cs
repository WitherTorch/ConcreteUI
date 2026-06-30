using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public delegate int CustomComputeDelegate(in LayoutContext context);

partial class LayoutNode
{
    private const int FixedValueCacheLimit = 256;

    private static readonly ConcurrentDictionary<int, LayoutNode> _fixedValueNodeDict = new ConcurrentDictionary<int, LayoutNode>();
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
            if (value > -FixedValueCacheLimit)
                return _smallValueNegativeNodes.AsUnsafeRef()[-value];
        }
        else
        {
            if (value < FixedValueCacheLimit)
                return _smallValuePositiveNodes.AsUnsafeRef()[value];
        }
        return _fixedValueNodeDict.GetOrAdd(value, static key => new FixedValueLayoutNode(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode Element(UIElement element, LayoutProperty property)
        => element.GetLayoutDefinition(property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode Custom(Func<int> computeFunc) => new SimpleCustomLayoutNode(computeFunc);

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
