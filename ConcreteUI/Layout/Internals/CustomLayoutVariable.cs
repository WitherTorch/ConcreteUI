using System;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class CustomLayoutVariable : LayoutVariable
    {
        private readonly Func<LayoutVariableManager, int> _func;

        public CustomLayoutVariable(Func<LayoutVariableManager, int> func)
        {
            _func = func;
        }

        public override int Compute(in LayoutVariableManager manager) => _func(manager);

        public override bool Equals(object? obj) => obj is CustomLayoutVariable another && _func == another._func;

        public override int GetHashCode() => _func.GetHashCode();
    }
}