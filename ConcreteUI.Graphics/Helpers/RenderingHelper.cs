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

        public static float FloorInPixel(float valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            return FloorInPixelCore(valueInPoints, pointsPerPixel);
        }

        public static PointF FloorInPixel(PointF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float x = FloorInPixelCore(valueInPoints.X, pointsPerPixel);
            float y = FloorInPixelCore(valueInPoints.Y, pointsPerPixel);
            return new PointF(x, y);
        }

        public static RectF FloorInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float left = FloorInPixelCore(valueInPoints.Left, pointsPerPixel);
            float top = FloorInPixelCore(valueInPoints.Top, pointsPerPixel);
            float right = FloorInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = FloorInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }

        public static float CeilingInPixel(float valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            return CeilingInPixelCore(valueInPoints, pointsPerPixel);
        }

        public static PointF CeilingInPixel(PointF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float x = CeilingInPixelCore(valueInPoints.X, pointsPerPixel);
            float y = CeilingInPixelCore(valueInPoints.Y, pointsPerPixel);
            return new PointF(x, y);
        }

        public static RectF CeilingInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float left = CeilingInPixelCore(valueInPoints.Left, pointsPerPixel);
            float top = CeilingInPixelCore(valueInPoints.Top, pointsPerPixel);
            float right = CeilingInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = CeilingInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }

        public static float RoundInPixel(float valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            return RoundInPixelCore(valueInPoints, pointsPerPixel);
        }

        public static PointF RoundInPixel(PointF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float x = RoundInPixelCore(valueInPoints.X, pointsPerPixel);
            float y = RoundInPixelCore(valueInPoints.Y, pointsPerPixel);
            return new PointF(x, y);
        }

        public static RectF RoundInPixel(in RectF valueInPoints, float pointsPerPixel)
        {
            if (pointsPerPixel == 1.0f)
                return valueInPoints;
            float left = RoundInPixelCore(valueInPoints.Left, pointsPerPixel);
            float top = RoundInPixelCore(valueInPoints.Top, pointsPerPixel);
            float right = RoundInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pointsPerPixel);
            float bottom = RoundInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pointsPerPixel);
            return new RectF(left, top, right, bottom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FloorInPixelCore(float  valueInPoints, float pointsPerPixel)
            => MathF.Floor(valueInPoints / pointsPerPixel) * pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CeilingInPixelCore(float  valueInPoints, float pointsPerPixel)
            => MathF.Ceiling(valueInPoints / pointsPerPixel) * pointsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float RoundInPixelCore(float  valueInPoints, float pointsPerPixel)
            => MathF.Round(valueInPoints / pointsPerPixel) * pointsPerPixel;
    }
}
