using System;
using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class SimpleCustomLayoutNode : LayoutNode
{
    private readonly Func<int> _func;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SimpleCustomLayoutNode(Func<int> func) => _func = func;

    protected override int ComputeCore(in LayoutContext context) => _func.Invoke();
}