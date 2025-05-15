namespace ConcreteUI.Layout.Internals
{
    internal sealed class NegativeOperatorLayoutVariable : LayoutVariable
    {
        private readonly LayoutVariable _variable;

        public NegativeOperatorLayoutVariable(LayoutVariable variable)
        {
            _variable = variable;
        }

        public override int Compute(in LayoutVariableManager manager)
            => -manager.GetComputedValue(_variable);

        public override bool Equals(object? obj) => obj is NegativeOperatorLayoutVariable another && _variable.Equals(another._variable);

        public override int GetHashCode() => _variable.GetHashCode();
    }
}
