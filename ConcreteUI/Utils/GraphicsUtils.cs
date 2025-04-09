﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Utils
{
    internal static class GraphicsUtils
    {
        [Inline(InlineBehavior.Remove)]
        public static Rect ConvertRectangle(in RectF rawRect)
        {
            return new Rect(MathI.Floor(rawRect.Left), MathI.Floor(rawRect.Top), MathI.Floor(rawRect.Right), MathI.Floor(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static Rect ConvertRectangle(in RectangleF rawRect)
        {
            return new Rect(MathI.Floor(rawRect.Left), MathI.Floor(rawRect.Top), MathI.Floor(rawRect.Right), MathI.Floor(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static Rect AdjustRectangle(in RectF rawRect)
        {
            return new Rect(MathI.Floor(rawRect.Left), MathI.Floor(rawRect.Top), MathI.Ceiling(rawRect.Right), MathI.Ceiling(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static Rect AdjustRectangle(in RectangleF rawRect)
        {
            return new Rect(MathI.Floor(rawRect.Left), MathI.Floor(rawRect.Top), MathI.Ceiling(rawRect.Right), MathI.Ceiling(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleF(in RectF rawRect)
        {
            return new RectF(MathF.Floor(rawRect.Left), MathF.Floor(rawRect.Top), MathF.Ceiling(rawRect.Right), MathF.Ceiling(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleF(in RectangleF rawRect)
        {
            return new RectF(MathF.Floor(rawRect.Left), MathF.Floor(rawRect.Top), MathF.Ceiling(rawRect.Right), MathF.Ceiling(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static RectangleF AdjustRectangleF2(RectangleF rawRect)
        {
            return RectangleF.FromLTRB(MathF.Floor(rawRect.Left), MathF.Floor(rawRect.Top), MathF.Ceiling(rawRect.Right), MathF.Ceiling(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleFAsBorderBounds(in RectF rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(MathF.Floor(rawRect.Left) + gap, MathF.Floor(rawRect.Top) + gap, MathF.Ceiling(rawRect.Right) - gap, MathF.Ceiling(rawRect.Bottom) - gap);
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleFAsBorderBounds(in RectangleF rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(MathF.Floor(rawRect.Left) + gap, MathF.Floor(rawRect.Top) + gap, MathF.Ceiling(rawRect.Right) - gap, MathF.Ceiling(rawRect.Bottom) - gap);
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleAsBorderBounds(in Rect rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF AdjustRectangleAsBorderBounds(in Rectangle rawRect, float lineWidth)
        {
            float gap = lineWidth * 0.5f;
            return new RectF(rawRect.Left + gap, rawRect.Top + gap, rawRect.Right - gap, rawRect.Bottom - gap);
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF RoundRectangleF(in RectF rawRect)
        {
            return new RectF(MathF.Round(rawRect.Left), MathF.Round(rawRect.Top), MathF.Round(rawRect.Right), MathF.Round(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static RectF RoundRectangleF(in RectangleF rawRect)
        {
            return new RectF(MathF.Round(rawRect.Left), MathF.Round(rawRect.Top), MathF.Round(rawRect.Right), MathF.Round(rawRect.Bottom));
        }

        [Inline(InlineBehavior.Remove)]
        public static Point AdjustPoint(in PointF rawPoint)
        {
            return new Point(MathI.Floor(rawPoint.X), MathI.Floor(rawPoint.Y));
        }

        [Inline(InlineBehavior.Remove)]
        public static PointF AdjustPointF(in PointF rawPoint)
        {
            return new PointF(MathF.Floor(rawPoint.X), MathF.Floor(rawPoint.Y));
        }

        [Inline(InlineBehavior.Remove)]
        public static PointF AdjustPointF2(in PointF rawPoint)
        {
            return new PointF(MathF.Floor(rawPoint.X), MathF.Ceiling(rawPoint.Y));
        }

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
        {
            return MeasureTextWidthCore(text, format);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MeasureTextHeight(string text, DWriteTextFormat format)
        {
            return MeasureTextHeightCore(text, format);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MeasureTextWidthAsInt(string text, DWriteTextFormat format)
        {
            return MathI.Ceiling(MeasureTextWidthCore(text, format));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MeasureTextHeightAsInt(string text, DWriteTextFormat format)
        {
            return MathI.Ceiling(MeasureTextHeightCore(text, format));
        }

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
            layout.MaxWidth = MathF.Ceiling(layout.GetMetrics().Width + extraWidth);
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
        public static bool CheckBrushIsSolid(D2D1SolidColorBrush brush) => brush.Color.A < 1.0f;

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
        public static void ClearAndFill(D2D1DeviceContext context, D2D1Brush brushToFill, in D2D1ColorF colorToClear)
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
