using System.Drawing;

using ConcreteUI.Internals;

using InlineMethod;

namespace ConcreteUI.Utils
{
    internal static class GenericExtensions
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsTrue(this long value) => value != BooleanConstants.FalseLong;

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool Contains(this Rectangle rectangle, in PointF point) => rectangle.Contains(point.X, point.Y);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool Contains(this Rectangle rectangle, float x, float y)
        {
            if (rectangle.X <= x && x < rectangle.Right && rectangle.Y <= y)
            {
                return y < rectangle.Bottom;
            }
            return false;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static uint ToBgra(this Color color)
        {
            unchecked
            {
                return ((uint)color.B << 24) + ((uint)color.G << 16) + ((uint)color.R << 8) + color.A;
            }
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static uint ToBgr(this Color color)
        {
            unchecked
            {
                return ((uint)color.B << 16) + ((uint)color.G << 8) + color.R;
            }
        }
    }
}
