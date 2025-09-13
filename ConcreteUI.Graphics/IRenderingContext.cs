using System;
using System.Drawing;
using System.Numerics;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics
{
    public interface IRenderingContext : IDisposable
    {
        SizeF Size { get; }

        void Clear();
        void Clear(in D2D1ColorF color);
        unsafe void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1, D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear, RectF* sourceRectangle = null);
        unsafe void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, RectF* sourceRectangle = null, Matrix4x4* perspectiveTransform = null);
        void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity = 1, D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear);
        void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity = 1, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear);
        void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity = 1, D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear);
        void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity = 1, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear);
        void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, in Matrix4x4 perspectiveTransform, float opacity = 1, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear);
        unsafe void DrawEllipse(D2D1Ellipse* ellipse, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        void DrawEllipse(in D2D1Ellipse ellipse, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        void DrawGeometry(D2D1Geometry geometry, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        void DrawImage(D2D1Image image, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver);
        void DrawImage(D2D1Image image, in PointF targetOffset, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver);
        void DrawImage(D2D1Image image, in PointF targetOffset, in RectF imageRectangle, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver);
        unsafe void DrawImage(D2D1Image image, PointF* targetOffset, RectF* imageRectangle, D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver);
        void DrawLine(PointF point0, PointF point1, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        void DrawRectangle(in RectF rect, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        unsafe void DrawRectangle(RectF* rect, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        unsafe void DrawRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        void DrawRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush, float strokeWidth = 1, D2D1StrokeStyle? strokeStyle = null);
        unsafe void DrawText(char* text, uint textLength, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural);
        void DrawText(char character, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural);
        unsafe void DrawText(char character, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural);
        void DrawText(string text, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural);
        unsafe void DrawText(string text, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural);
        void DrawTextLayout(PointF origin, DWriteTextLayout textLayout, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None);
        unsafe void FillEllipse(D2D1Ellipse* ellipse, D2D1Brush brush);
        void FillEllipse(in D2D1Ellipse ellipse, D2D1Brush brush);
        void FillGeometry(D2D1Geometry geometry, D2D1Brush brush, D2D1Brush? opacityBrush = null);
        void FillRectangle(in RectF rect, D2D1Brush brush);
        unsafe void FillRectangle(RectF* rect, D2D1Brush brush);
        unsafe void FillRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush);
        void FillRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush);
    }
}