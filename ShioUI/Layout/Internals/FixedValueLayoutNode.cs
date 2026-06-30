using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class FixedValueLayoutNode : LayoutNode
{
    private readonly int _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedValueLayoutNode(int value) => _value = value;

    public int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    protected override int ComputeCore(in LayoutContext context) => _value;
}
