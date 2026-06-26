using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public abstract partial class LayoutNode
{
    public static readonly LayoutNode Empty = new FixedValueLayoutNode(0);

    public bool IsEmpty => ReferenceEquals(this, Empty) || (this is FixedValueLayoutNode fixedVariable && fixedVariable.Value == 0);

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
}
