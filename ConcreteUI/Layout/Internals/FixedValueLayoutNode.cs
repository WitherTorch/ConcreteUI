using System.Runtime.CompilerServices;

namespace ConcreteUI.Layout.Internals;

internal sealed class FixedValueLayoutNode : LayoutNode
{
    private readonly int _value;

    public FixedValueLayoutNode(int value)
    {
        _value = value;
    }

    public int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    public override int Compute(in LayoutNodeManager manager) => _value;

    public override bool Equals(object? obj) => obj is FixedValueLayoutNode fixedVariable && fixedVariable._value == _value;

    public override int GetHashCode() => _value;
}
