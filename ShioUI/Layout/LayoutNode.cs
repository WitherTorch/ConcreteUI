using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public abstract partial class LayoutNode
{
    public static readonly LayoutNode Empty = new FixedValueLayoutNode(0);

    private static int _identifierCounter = 0;

    private readonly int _identifier;

    public int NodeId => _identifier;

    public bool IsEmpty => ReferenceEquals(this, Empty) || (this is FixedValueLayoutNode fixedVariable && fixedVariable.Value == 0);

    protected LayoutNode() => _identifier = InterlockedHelper.Increment(ref _identifierCounter);

    public abstract int Compute(in LayoutNodeManager manager);

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
