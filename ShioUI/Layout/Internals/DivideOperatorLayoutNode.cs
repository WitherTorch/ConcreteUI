using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class DivideOperatorLayoutNode : LayoutNode
{
    private readonly LayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DivideOperatorLayoutNode(LayoutNode left, LayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override int ComputeCore(in LayoutContext context)
        => context.GetComputedValue(_leftVariable) / context.GetComputedValue(_rightVariable);
}
