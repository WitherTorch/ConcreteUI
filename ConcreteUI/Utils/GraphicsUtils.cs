using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Utils
{
    public static class GraphicsUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF AdjustRectangleFAsBorderBounds(in RectF rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF AdjustRectangleFAsBorderBounds(in RectangleF rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF AdjustRectangleAsBorderBounds(in Rect rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF AdjustRectangleAsBorderBounds(in Rectangle rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF RoundRectangleF(in RectF rawRect)
            => new RectF(MathF.Round(rawRect.Left), MathF.Round(rawRect.Top), MathF.Round(rawRect.Right), MathF.Round(rawRect.Bottom));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF RoundRectangleF(in RectangleF rawRect)
            => new RectF(MathF.Round(rawRect.Left), MathF.Round(rawRect.Top), MathF.Round(rawRect.Right), MathF.Round(rawRect.Bottom));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point AdjustPoint(in PointF rawPoint)
            => new Point(MathI.Floor(rawPoint.X), MathI.Floor(rawPoint.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF AdjustPointF(in PointF rawPoint)
            => new PointF(MathF.Floor(rawPoint.X), MathF.Floor(rawPoint.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF AdjustPointF2(in PointF rawPoint)
            => new PointF(MathF.Floor(rawPoint.X), MathF.Ceiling(rawPoint.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size ScalingSize(SizeF original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new Size(MathI.Floor(original.Width * factorX), MathI.Floor(original.Height * factorY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizeF ScalingSize(Size original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new SizeF(original.Width * factorX, original.Height * factorY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point ScalingPoint(Point original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new Point(MathI.Floor(original.X * factorX), MathI.Floor(original.Y * factorY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF ScalingPoint(PointF original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new PointF(original.X * factorX, original.Y * factorY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point ScalingPointAndConvert(PointF original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new Point(MathI.Floor(original.X * factorX), MathI.Floor(original.Y * factorY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointF ScalingPointAndConvert(Point original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new PointF(original.X * factorX, original.Y * factorY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect ScalingRect(Rect original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            if (factorX == 1.0f && factorY == 1.0f)
                return original;
            return new Rect(left: MathI.FloorPositive(original.Left * factorX),
                top: MathI.FloorPositive(original.Top * factorY),
                right: MathI.Ceiling(original.Right * factorX),
                bottom: MathI.Ceiling(original.Bottom * factorY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF ScalingRect(RectF original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            if (factorX == 1.0f && factorY == 1.0f)
                return original;
            return new RectF(left: original.Left * factorX,
                top: original.Top * factorY,
                right: original.Right * factorX,
                bottom: original.Bottom * factorY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect ScalingRectAndConvert(RectF original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new Rect(left: MathI.FloorPositive(original.Left * factorX),
                top: MathI.FloorPositive(original.Top * factorY),
                right: MathI.Ceiling(original.Right * factorX),
                bottom: MathI.Ceiling(original.Bottom * factorY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectF ScalingRectAndConvert(Rect original, Vector2 scaleFactor)
        {
            (float factorX, float factorY) = scaleFactor;
            return new RectF(left: original.Left * factorX,
                top: original.Top * factorY,
                right: original.Right * factorX,
                bottom: original.Bottom * factorY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size AdjustSize(Size original, SizeF min, SizeF max, Vector2 pixelsPerPoint)
        {
            if (max == SizeF.Empty)
            {
                if (min == SizeF.Empty)
                    return original;
                return Max(original, ScalingSize(min, pixelsPerPoint));
            }
            else
            {
                if (min == SizeF.Empty)
                    return Min(original, ScalingSize(min, pixelsPerPoint));
                return Clamp(original, ScalingSize(min, pixelsPerPoint), ScalingSize(min, pixelsPerPoint));
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static Size Min(Size original, Size min)
            => new Size(width: MathHelper.Min(original.Width, min.Width),
                height: MathHelper.Min(original.Height, min.Height));

        [Inline(InlineBehavior.Remove)]
        private static Size Max(Size original, Size max)
            => new Size(width: MathHelper.Max(original.Width, max.Width),
                height: MathHelper.Max(original.Height, max.Height));

        [Inline(InlineBehavior.Remove)]
        private static Size Clamp(Size original, Size min, Size max)
            => new Size(width: MathHelper.Clamp(original.Width, min.Width, max.Width),
                height: MathHelper.Clamp(original.Height, min.Height, max.Height));

        [Inline(InlineBehavior.Remove)]
        private static float MeasureTextWidthCore(string text, DWriteTextFormat format)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, float.PositiveInfinity);
            if (layout is null)
                return 0.0f;
            float result = layout.GetMetrics().Width;
            layout.Dispose();
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private static float MeasureTextHeightCore(string text, DWriteTextFormat format)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, float.PositiveInfinity);
            if (layout is null)
                return 0.0f;
            float result = layout.GetMetrics().Height;
            layout.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MeasureTextWidth(string text, DWriteTextFormat format)
            => MeasureTextWidthCore(text, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MeasureTextHeight(string text, DWriteTextFormat format)
            => MeasureTextHeightCore(text, format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MeasureTextWidthAsInt(string text, DWriteTextFormat format)
            => MathI.Ceiling(MeasureTextWidthCore(text, format));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MeasureTextHeightAsInt(string text, DWriteTextFormat format)
            => MathI.Ceiling(MeasureTextHeightCore(text, format));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SizeF MeasureTextSizeF(string text, DWriteTextFormat format)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, float.PositiveInfinity);
            if (layout is null)
                return default;
            DWriteTextMetrics metrics = layout.GetMetrics();
            SizeF result = new SizeF(metrics.Width, metrics.Height);
            layout.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size MeasureTextSize(string text, DWriteTextFormat format)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, float.PositiveInfinity);
            if (layout is null)
                return default;
            DWriteTextMetrics metrics = layout.GetMetrics();
            Size result = new Size(MathI.Ceiling(metrics.Width), MathI.Ceiling(metrics.Height));
            layout.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DWriteTextLayout CreateCustomTextLayout(string text, DWriteTextFormat format, float extraWidth, float itemHeight)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, itemHeight);
            DWriteTextMetrics metrics = layout.GetMetrics();
            layout.MaxWidth = MathF.Ceiling(metrics.Width + extraWidth);
            if (float.IsInfinity(itemHeight))
                layout.MaxHeight = metrics.Height;
            return layout;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DWriteTextLayout CreateCustomTextLayout(string text, DWriteTextFormat format, float itemHeight)
        {
            DWriteTextLayout layout = SharedResources.DWriteFactory.CreateTextLayout(text, format, float.PositiveInfinity, itemHeight);
            DWriteTextMetrics metrics = layout.GetMetrics();
            layout.MaxWidth = MathF.Ceiling(metrics.Width);
            if (float.IsInfinity(itemHeight))
                layout.MaxHeight = metrics.Height;
            return layout;
        }

        public static bool CheckBrushIsSolid(D2D1Brush brush) => brush switch
        {
            D2D1SolidColorBrush castedBrush => CheckBrushIsSolid(castedBrush),
            D2D1LinearGradientBrush castedBrush => CheckBrushIsSolid(castedBrush),
            D2D1RadialGradientBrush castedBrush => CheckBrushIsSolid(castedBrush),
            _ => false,
        };

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool CheckBrushIsSolid(D2D1SolidColorBrush brush) => brush.Color.A >= 1.0f;

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool CheckBrushIsSolid(D2D1LinearGradientBrush brush)
        {
            D2D1GradientStopCollection? collection = brush.GradientStopCollection;
            if (collection is null)
                return false;
            foreach (D2D1GradientStop stop in collection)
            {
                if (stop.Color.A < 1.0f)
                    return false;
            }
            return true;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool CheckBrushIsSolid(D2D1RadialGradientBrush brush)
        {
            D2D1GradientStopCollection? collection = brush.GradientStopCollection;
            if (collection is null)
                return false;
            foreach (D2D1GradientStop stop in collection)
            {
                if (stop.Color.A < 1.0f)
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAndFill(in RegionalRenderingContext context, D2D1Brush brushToFill, in D2D1ColorF colorToClear)
        {
            if (brushToFill is null)
            {
                context.Clear(colorToClear);
                return;
            }
            if (CheckBrushIsSolid(brushToFill))
            {
                if (brushToFill is D2D1SolidColorBrush solidColorBrush)
                {
                    context.Clear(solidColorBrush.Color);
                    return;
                }
                context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), brushToFill);
                return;
            }
            context.Clear(colorToClear);
            context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), brushToFill);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAndFill(IRenderingContext context, D2D1Brush brushToFill, in D2D1ColorF colorToClear)
        {
            if (brushToFill is null)
            {
                context.Clear(colorToClear);
                return;
            }
            if (CheckBrushIsSolid(brushToFill))
            {
                if (brushToFill is D2D1SolidColorBrush solidColorBrush)
                {
                    context.Clear(solidColorBrush.Color);
                    return;
                }
                context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), brushToFill);
                return;
            }
            context.Clear(colorToClear);
            context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), brushToFill);
        }
    }
}
