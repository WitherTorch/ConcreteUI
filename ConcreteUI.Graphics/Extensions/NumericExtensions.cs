using InlineMethod;

namespace ConcreteUI.Graphics.Extensions
{
    internal static class NumericExtensions
    {
        private const float CeilConstantSingle = 0.999999f;
        private const double CeilConstantDouble = 0.999999999999999d;
        private const int NumberLimit = 32768;
        private const float NumberLimitSingle = NumberLimit;
        private const double NumberLimitDouble = NumberLimit;

        [Inline(InlineBehavior.Remove)]
        public static int MakeSigned(this uint value) => unchecked((int)(value & int.MaxValue));

        [Inline(InlineBehavior.Remove)]
        public static uint MakeUnsigned(this int value) => unchecked((uint)(value & int.MaxValue));

        [Inline(InlineBehavior.Remove)]
        public static int CeilingToInt(this float val)
        {
            if (val > NumberLimitSingle)
                return (int)(val + CeilConstantSingle);
            return NumberLimit - (int)(NumberLimitSingle - val);
        }

        [Inline(InlineBehavior.Remove)]
        public static int CeilingToInt(this double val)
        {
            if (val > NumberLimitDouble)
                return (int)(val + CeilConstantDouble);
            return NumberLimit - (int)(NumberLimitDouble - val);
        }

        [Inline(InlineBehavior.Remove)]
        public static int FloorToInt(this float value)
        {
            if (value < NumberLimitSingle)
            {
                if (value < 0.0)
                    return (int)value - 1;
                return (int)(value + NumberLimitSingle) - NumberLimit;
            }
            return (int)value;
        }

        [Inline(InlineBehavior.Remove)]
        public static int FloorToInt(this double value)
        {
            if (value < NumberLimitDouble)
            {
                if (value < 0.0)
                    return (int)value - 1;
                return (int)(value + NumberLimitDouble) - NumberLimit;
            }
            return (int)value;
        }
    }
}
