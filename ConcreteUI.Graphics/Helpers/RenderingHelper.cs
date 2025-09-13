using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Helpers
{
    public static class RenderingHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDefaultBorderWidth(float pointsPerPixel)
            => MathF.Round(pointsPerPixel) / pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloorInPixel(float valueInPoints, float pointsPerPixel)
            => MathF.Floor(valueInPoints / pointsPerPixel) * pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF FloorInPixel(PointF valueInPoints, float pointsPerPixel)
            => new PointF(FloorInPixel(valueInPoints.X, pointsPerPixel), FloorInPixel(valueInPoints.Y, pointsPerPixel));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF FloorInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            float left = FloorInPixel(valueInPoints.Left, pointsPerPixel);
            float top = FloorInPixel(valueInPoints.Top, pointsPerPixel);
            float right = FloorInPixel(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = FloorInPixel(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CeilingInPixel(float valueInPoints, float pointsPerPixel)
            => MathF.Ceiling(valueInPoints / pointsPerPixel) * pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF CeilingInPixel(PointF valueInPoints, float pointsPerPixel)
            => new PointF(CeilingInPixel(valueInPoints.X, pointsPerPixel), CeilingInPixel(valueInPoints.Y, pointsPerPixel));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF CeilingInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            float left = CeilingInPixel(valueInPoints.Left, pointsPerPixel);
            float top = CeilingInPixel(valueInPoints.Top, pointsPerPixel);
            float right = CeilingInPixel(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = CeilingInPixel(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundInPixel(float valueInPoints, float pointsPerPixel)
            => MathF.Round(valueInPoints / pointsPerPixel) * pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF RoundInPixel(PointF valueInPoints, float pointsPerPixel)
            => new PointF(RoundInPixel(valueInPoints.X, pointsPerPixel), RoundInPixel(valueInPoints.Y, pointsPerPixel));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF RoundInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            float left = RoundInPixel(valueInPoints.Left, pointsPerPixel);
            float top = RoundInPixel(valueInPoints.Top, pointsPerPixel);
            float right = RoundInPixel(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = RoundInPixel(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }
    }
}
