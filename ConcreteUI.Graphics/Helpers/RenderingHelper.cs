using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Helpers
{
    public static class RenderingHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDefaultBorderWidth(float pixelsPerPoint)
            => MathF.Round(pixelsPerPoint, MidpointRounding.AwayFromZero) / pixelsPerPoint;

        public static float FloorInPixel(float valueInPoints, float pixelsPerPoint)
        {
            if (pixelsPerPoint == 1.0f)
                return valueInPoints;
            return FloorInPixelCore(valueInPoints, pixelsPerPoint);
        }

        public static PointF FloorInPixel(Point valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = FloorInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = FloorInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static PointF FloorInPixel(PointF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = FloorInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = FloorInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static RectF FloorInPixel(in Rect valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return (RectF)valueInPoints;
            float left = FloorInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = FloorInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = FloorInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = FloorInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        public static RectF FloorInPixel(in RectF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float left = FloorInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = FloorInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = FloorInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = FloorInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        public static float CeilingInPixel(float valueInPoints, float pixelsPerPoint)
        {
            if (pixelsPerPoint == 1.0f)
                return valueInPoints;
            return CeilingInPixelCore(valueInPoints, pixelsPerPoint);
        }

        public static PointF CeilingInPixel(Point valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = CeilingInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = CeilingInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static PointF CeilingInPixel(PointF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = CeilingInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = CeilingInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static RectF CeilingInPixel(in Rect valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return (RectF)valueInPoints;
            float left = CeilingInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = CeilingInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = CeilingInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = CeilingInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        public static RectF CeilingInPixel(in RectF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float left = CeilingInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = CeilingInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = CeilingInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = CeilingInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        public static float RoundInPixel(float valueInPoints, float pixelsPerPoint)
        {
            if (pixelsPerPoint == 1.0f)
                return valueInPoints;
            return RoundInPixelCore(valueInPoints, pixelsPerPoint);
        }

        public static PointF RoundInPixel(Point valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = RoundInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = RoundInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static PointF RoundInPixel(PointF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float x = RoundInPixelCore(valueInPoints.X, pixelsPerPoint.X);
            float y = RoundInPixelCore(valueInPoints.Y, pixelsPerPoint.Y);
            return new PointF(x, y);
        }

        public static RectF RoundInPixel(in Rect valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return (RectF)valueInPoints;
            float left = RoundInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = RoundInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = RoundInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = RoundInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        public static RectF RoundInPixel(in RectF valueInPoints, Vector2 pixelsPerPoint)
        {
            if (pixelsPerPoint == Vector2.One)
                return valueInPoints;
            float left = RoundInPixelCore(valueInPoints.Left, pixelsPerPoint.X);
            float top = RoundInPixelCore(valueInPoints.Top, pixelsPerPoint.Y);
            float right = RoundInPixelCore(valueInPoints.Right + (left - valueInPoints.Left), pixelsPerPoint.X);
            float bottom = RoundInPixelCore(valueInPoints.Bottom + (top - valueInPoints.Top), pixelsPerPoint.Y);
            return new RectF(left, top, right, bottom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FloorInPixelCore(float  valueInPoints, float pixelsPerPoint)
            => MathF.Floor(valueInPoints / pixelsPerPoint) * pixelsPerPoint;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CeilingInPixelCore(float  valueInPoints, float pixelsPerPoint)
            => MathF.Ceiling(valueInPoints / pixelsPerPoint) * pixelsPerPoint;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float RoundInPixelCore(float  valueInPoints, float pixelsPerPoint)
            => MathF.Round(valueInPoints / pixelsPerPoint, MidpointRounding.AwayFromZero) * pixelsPerPoint;
    }
}
