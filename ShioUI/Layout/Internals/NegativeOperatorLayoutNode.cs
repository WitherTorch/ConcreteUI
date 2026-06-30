using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class NegativeOperatorLayoutNode : LayoutNode
{
    private readonly LayoutNode _variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NegativeOperatorLayoutNode(LayoutNode variable) => _variable = variable;

    protected override int ComputeCore(in LayoutContext context)
        => -context.GetComputedValue(_variable);
}
