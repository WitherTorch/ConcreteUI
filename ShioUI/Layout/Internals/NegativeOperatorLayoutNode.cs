namespace ShioUI.Layout.Internals;

internal sealed class NegativeOperatorLayoutNode : LayoutNode
{
    private readonly LayoutNode _variable;

    public NegativeOperatorLayoutNode(LayoutNode variable)
    {
        _variable = variable;
    }

    protected override int ComputeCore(in LayoutNodeManager manager)
        => -manager.GetComputedValue(_variable);

    public override bool Equals(object? obj) => obj is NegativeOperatorLayoutNode another && _variable.Equals(another._variable);

    public override int GetHashCode() => _variable.GetHashCode();
}
