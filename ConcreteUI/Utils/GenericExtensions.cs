using System.Drawing;
using System.Runtime.CompilerServices;

using InlineMethod;

using WitherTorch.Common;

namespace ConcreteUI.Utils
{
    internal static class GenericExtensions
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTrue(this long value) => value != Booleans.FalseLong;

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Rectangle rectangle, in PointF point) => rectangle.Contains(point.X, point.Y);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Rectangle rectangle, float x, float y)
        {
            if (rectangle.X <= x && x < rectangle.Right && rectangle.Y <= y)
            {
                return y < rectangle.Bottom;
            }
            return false;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToBgra(this Color color)
        {
            unchecked
            {
                return ((uint)color.B << 24) + ((uint)color.G << 16) + ((uint)color.R << 8) + color.A;
            }
        }

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToBgr(this Color color)
        {
            unchecked
            {
                return ((uint)color.B << 16) + ((uint)color.G << 8) + color.R;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this Rectangle _this) 
            => _this.Location.IsValid() && _this.Size.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this Point _this)
            => _this.X >= 0 && _this.Y >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this Size _this) 
            => _this.Width >= 0 && _this.Height >= 0;
    }
}
