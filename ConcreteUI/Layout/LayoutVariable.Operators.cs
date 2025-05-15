using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Layout.Internals;

namespace ConcreteUI.Layout
{
    partial class LayoutVariable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator LayoutVariable(int value) => Fixed(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator +(LayoutVariable variable) => variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator -(LayoutVariable variable)
        {
            if (variable.IsEmpty)
                return variable;
            if (variable is FixedLayoutVariable fixedVariable)
                return Fixed(-fixedVariable.Value);
            return new NegativeOperatorLayoutVariable(variable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator +(LayoutVariable left, LayoutVariable right)
        {
            if (left.IsEmpty)
                return right;
            if (right.IsEmpty)
                return left;
            if (left is FixedLayoutVariable fixedLeft && right is FixedLayoutVariable fixedRight)
                return Fixed(fixedLeft.Value + fixedRight.Value);
            return new AddOperatorLayoutVariable(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator -(LayoutVariable left, LayoutVariable right)
        {
            if (ReferenceEquals(left, right))
                return _empty;
            if (left.IsEmpty)
                return -right;
            if (right.IsEmpty)
                return left;
            if (left is FixedLayoutVariable fixedLeft && right is FixedLayoutVariable fixedRight)
                return Fixed(fixedLeft.Value - fixedRight.Value);
            return new SubtractOperatorLayoutVariable(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator *(LayoutVariable left, LayoutVariable right)
        {
            if (left.IsEmpty || right.IsOne())
                return left;
            if (right.IsEmpty || left.IsOne())
                return right;
            if (left is FixedLayoutVariable fixedLeft && right is FixedLayoutVariable fixedRight)
                return Fixed(fixedLeft.Value * fixedRight.Value);
            return new MultiplyOperatorLayoutVariable(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutVariable operator /(LayoutVariable left, LayoutVariable right)
        {
            if (right.IsEmpty)
                throw new DivideByZeroException();
            if (ReferenceEquals(left, right))
                return Fixed(1);
            if (left.IsEmpty || right.IsOne())
                return left;
            if (left is FixedLayoutVariable fixedLeft && right is FixedLayoutVariable fixedRight)
                return Fixed(fixedLeft.Value / fixedRight.Value);
            return new DivideOperatorLayoutVariable(left, right);
        }

        private bool IsOne() => this is FixedLayoutVariable fixedVariable && fixedVariable.Value == 1;
    }
}
