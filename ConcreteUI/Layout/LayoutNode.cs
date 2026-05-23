using System.Runtime.CompilerServices;

using ConcreteUI.Layout.Internals;

namespace ConcreteUI.Layout
{
    public abstract partial class LayoutNode
    {
        private static readonly LayoutNode _empty = new FixedValueLayoutNode(0);

        public static LayoutNode Empty => _empty;

        public bool IsEmpty => ReferenceEquals(this, _empty) || (this is FixedValueLayoutNode fixedVariable && fixedVariable.Value == 0);

        public abstract int Compute(in LayoutNodeManager manager);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutNode Negative() => -this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutNode Add(LayoutNode variable) => this + variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutNode Subtract(LayoutNode variable) => this - variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutNode Multiply(LayoutNode variable) => this * variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayoutNode Divide(LayoutNode variable) => this / variable;
    }
}
