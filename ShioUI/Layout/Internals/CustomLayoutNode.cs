using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class CustomLayoutNode : LayoutNode
{
    private readonly CustomComputeDelegate _func;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CustomLayoutNode(CustomComputeDelegate func) => _func = func;

    protected override int ComputeCore(in LayoutContext context) => _func.Invoke(in context);
}