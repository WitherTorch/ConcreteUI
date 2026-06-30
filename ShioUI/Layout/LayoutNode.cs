using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public abstract partial class LayoutNode
{
    public static readonly LayoutNode Empty = new FixedValueLayoutNode(0);

    private static int _identifierCounter = 0;

    private readonly int _identifier;
    private ulong _layoutTimestamp;
    private int _cachedResult;

    public int NodeId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _identifier;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReferenceEquals(this, Empty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected LayoutNode() => _identifier = InterlockedHelper.Increment(ref _identifierCounter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compute(in LayoutContext context)
        => context.GetComputedValue(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int ComputeInternal(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (timestamp != 0 && _layoutTimestamp == timestamp)
            return _cachedResult;
        int result = ComputeCore(context);
        _layoutTimestamp = timestamp;
        _cachedResult = result;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal (int Result, bool Cached) ComputeInternalWithCached(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (timestamp != 0 && _layoutTimestamp == timestamp)
            return (Result: _cachedResult, Cached: true);
        int result = ComputeCore(context);
        _layoutTimestamp = timestamp;
        _cachedResult = result;
        return (Result: result, Cached: false);
    }

    protected abstract int ComputeCore(in LayoutContext context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearCache() => _layoutTimestamp = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Negative() => -this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Add(LayoutNode variable) => this + variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Subtract(LayoutNode variable) => this - variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Multiply(LayoutNode variable) => this * variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Divide(LayoutNode variable) => this / variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Max(LayoutNode variable) => Max(this, variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Min(LayoutNode variable) => Min(this, variable);

    public override int GetHashCode() => _identifier;
}
