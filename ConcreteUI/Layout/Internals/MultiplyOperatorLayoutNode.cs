namespace ConcreteUI.Layout.Internals
{
    internal sealed class MultiplyOperatorLayoutNode : LayoutNode
    {
        private readonly LayoutNode _leftVariable, _rightVariable;

        public MultiplyOperatorLayoutNode(LayoutNode left, LayoutNode right)
        {
            _leftVariable = left;
            _rightVariable = right;
        }

        public override int Compute(in LayoutNodeManager manager)
            => manager.GetComputedValue(_leftVariable) * manager.GetComputedValue(_rightVariable);

        public override bool Equals(object? obj) => obj is MultiplyOperatorLayoutNode another &&
            _leftVariable.Equals(another._leftVariable) && _rightVariable.Equals(another._rightVariable);

        public override int GetHashCode() => _leftVariable.GetHashCode() ^ _rightVariable.GetHashCode();
    }
}
