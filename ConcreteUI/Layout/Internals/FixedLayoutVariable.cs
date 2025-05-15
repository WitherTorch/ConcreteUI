using System.Runtime.CompilerServices;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class FixedLayoutVariable : LayoutVariable
    {
        private readonly int _value;

        public FixedLayoutVariable(int value)
        {
            _value = value;
        }
       
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        public override int Compute(in LayoutVariableManager manager) => _value;

        public override bool Equals(object? obj) => obj is FixedLayoutVariable fixedVariable && fixedVariable._value == _value;

        public override int GetHashCode() => _value;
    }
}
