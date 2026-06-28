using RiceTea.Core.Helpers;

namespace ShioUI.Layout.Internals;

internal sealed class MinLayoutNode : LayoutNode
{
    private readonly LayoutNode _leftVariable, _rightVariable;

    public MinLayoutNode(LayoutNode left, LayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override int ComputeCore(in LayoutNodeManager manager)
        => MathHelper.Min(manager.GetComputedValue(_leftVariable), manager.GetComputedValue(_rightVariable));

    public override bool Equals(object? obj) => obj is MinLayoutNode another &&
        _leftVariable.Equals(another._leftVariable) && _rightVariable.Equals(another._rightVariable);

    public override int GetHashCode() => _leftVariable.GetHashCode() ^ _rightVariable.GetHashCode();
}
