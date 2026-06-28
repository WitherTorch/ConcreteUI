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

    public int NodeId => _identifier;

    public bool IsEmpty => ReferenceEquals(this, Empty) || (this is FixedValueLayoutNode fixedVariable && fixedVariable.Value == 0);

    protected LayoutNode() => _identifier = InterlockedHelper.Increment(ref _identifierCounter);

    public int Compute(in LayoutNodeManager manager, ulong timestamp)
    {
        if (_layoutTimestamp == timestamp)
            return _cachedResult;
        int result = ComputeCore(manager);
        _layoutTimestamp = timestamp;
        _cachedResult = result;
        return result;
    }

    protected abstract int ComputeCore(in LayoutNodeManager manager);

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

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => _identifier;
}
