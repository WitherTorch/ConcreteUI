using System.Runtime.CompilerServices;

using ConcreteUI.Layout.Internals;

namespace ConcreteUI.Layout
{
    public abstract partial class LayoutVariable
    {
        private static readonly LayoutVariable _empty = new FixedLayoutVariable(0);

        public static LayoutVariable Empty => _empty;

        public bool IsEmpty => ReferenceEquals(this, _empty) || (this is FixedLayoutVariable fixedVariable && fixedVariable.Value == 0);

        public abstract int Compute(in LayoutVariableManager manager);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable Negative() => -this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable Add(LayoutVariable variable) => this + variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable Subtract(LayoutVariable variable) => this - variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable Multiply(LayoutVariable variable) => this * variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutVariable Divide(LayoutVariable variable) => this / variable;
    }
}
