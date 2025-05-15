namespace ConcreteUI.Layout.Internals
{
    internal sealed class MultiplyOperatorLayoutVariable : LayoutVariable
    {
        private readonly LayoutVariable _leftVariable, _rightVariable;

        public MultiplyOperatorLayoutVariable(LayoutVariable left, LayoutVariable right)
        {
            _leftVariable = left;
            _rightVariable = right;
        }

        public override int Compute(in LayoutVariableManager manager)
            => manager.GetComputedValue(_leftVariable) * manager.GetComputedValue(_rightVariable);

        public override bool Equals(object? obj) => obj is MultiplyOperatorLayoutVariable another &&
            _leftVariable.Equals(another._leftVariable) && _rightVariable.Equals(another._rightVariable);

        public override int GetHashCode() => _leftVariable.GetHashCode() ^ _rightVariable.GetHashCode();
    }
}
