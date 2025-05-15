using WitherTorch.Common.Helpers;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class MinLayoutVariable : LayoutVariable
    {
        private readonly LayoutVariable _leftVariable, _rightVariable;

        public MinLayoutVariable(LayoutVariable left, LayoutVariable right)
        {
            _leftVariable = left;
            _rightVariable = right;
        }

        public override int Compute(in LayoutVariableManager manager)
            => MathHelper.Min(manager.GetComputedValue(_leftVariable), manager.GetComputedValue(_rightVariable));

        public override bool Equals(object? obj) => obj is MinLayoutVariable another &&
            _leftVariable.Equals(another._leftVariable) && _rightVariable.Equals(another._rightVariable);

        public override int GetHashCode() => _leftVariable.GetHashCode() ^ _rightVariable.GetHashCode();
    }
}
