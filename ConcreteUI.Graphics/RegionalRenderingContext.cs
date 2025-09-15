using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct RegionalRenderingContext : IRenderingContext
    {
        private readonly D2D1DeviceContext _context;
        private readonly DirtyAreaCollector _collector;
        private readonly RenderingClipToken _clipToken;
        private readonly Matrix3x2 _originalTransform;
        private readonly PointF _offsetPoint;
        private readonly float _pointsPerPixel;
        private readonly bool _isPixelAligned;

        private bool _disposed;

        public readonly D2D1DeviceContext DeviceContext => _context;

        public readonly RectF OriginalBounds => RectF.FromXYWH(_offsetPoint, Size);

        public readonly PointF OriginalPoint => _offsetPoint;

        public readonly RectF Bounds => RectF.FromXYWH(PointF.Empty, Size);

        public readonly SizeF Size => _clipToken.ClipRect.Size;

        public readonly float DefaultBorderWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RenderingHelper.GetDefaultBorderWidth(_pointsPerPixel);
        }

        public readonly float PointsPerPixel => _pointsPerPixel;

        public readonly bool HasDirtyCollector => !_collector.IsEmptyInstance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RegionalRenderingContext(scoped in RegionalRenderingContext original)
        {
            _context = original._context;
            _collector = original._collector;
            _clipToken = original._clipToken;
            _offsetPoint = original._offsetPoint;
            _pointsPerPixel = original._pointsPerPixel;
            _isPixelAligned = original._isPixelAligned;
            _originalTransform = original._originalTransform;
            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RegionalRenderingContext(D2D1DeviceContext context, DirtyAreaCollector collector, float pointsPerPixel,
            scoped in RectF clipRect, D2D1AntialiasMode antialiasMode, bool isPixelAligned)
        {
            _context = context;
            _collector = collector;
            _pointsPerPixel = pointsPerPixel;
            _clipToken = new RenderingClipToken(context, in clipRect, antialiasMode);

            Matrix3x2 transformMatrix = context.Transform;
            _originalTransform = transformMatrix;
            Vector2 translation = transformMatrix.Translation;
            translation += new Vector2(clipRect.X, clipRect.Y);
            transformMatrix.Translation = translation;
            _offsetPoint = new PointF(translation.X, translation.Y);
            context.Transform = transformMatrix;
            _isPixelAligned = isPixelAligned;
            _disposed = false;
        }

        public static RegionalRenderingContext Create(D2D1DeviceContext context, DirtyAreaCollector collector, float pointsPerPixel,
            in RectF clipRect, D2D1AntialiasMode antialiasMode, out RectF actualClipRect)
        {
            actualClipRect = RenderingHelper.RoundInPixel(in clipRect, pointsPerPixel);
            return new RegionalRenderingContext(context, collector, pointsPerPixel, in actualClipRect, antialiasMode, isPixelAligned: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear() => _context.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Clear(in D2D1ColorF color) => _context.Clear(in color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawLine(PointF point0, PointF point1, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null) => _context.DrawLine(point0, point1, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBorder(D2D1Brush brush, D2D1StrokeStyle? strokeStyle = null)
        {
            RectF borderRect = GetBorderRect(out float strokeWidth);
            _context.DrawRectangle(borderRect, brush, strokeWidth, strokeStyle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBorder(in RectF rect, D2D1Brush brush, D2D1StrokeStyle? strokeStyle = null)
        {
            RectF borderRect = GetBorderRect(rect, out float strokeWidth);
            _context.DrawRectangle(borderRect, brush, strokeWidth, strokeStyle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawBorder(RectF* rect, D2D1Brush brush, D2D1StrokeStyle? strokeStyle = null)
        {
            RectF borderRect = GetBorderRect(*rect, out float strokeWidth);
            _context.DrawRectangle(borderRect, brush, strokeWidth, strokeStyle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawRectangle(in RectF rect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawRectangle(rect, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawRectangle(RectF* rect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawRectangle(rect, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FillRectangle(in RectF rect, D2D1Brush brush)
            => _context.FillRectangle(rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void FillRectangle(RectF* rect, D2D1Brush brush)
            => _context.FillRectangle(rect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawRoundedRectangle(roundedRect, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawRoundedRectangle(roundedRect, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FillRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush)
            => _context.FillRoundedRectangle(roundedRect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void FillRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush)
            => _context.FillRoundedRectangle(roundedRect, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawEllipse(in D2D1Ellipse ellipse, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawEllipse(ellipse, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawEllipse(D2D1Ellipse* ellipse, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawEllipse(ellipse, brush, strokeWidth, strokeStyle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FillEllipse(in D2D1Ellipse ellipse, D2D1Brush brush)
            => _context.FillEllipse(ellipse, brush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void FillEllipse(D2D1Ellipse* ellipse, D2D1Brush brush)
            => _context.FillEllipse(ellipse, brush);

        /// <inheritdoc cref="D2D1RenderTarget.DrawGeometry(D2D1Geometry, D2D1Brush, float, D2D1StrokeStyle?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawGeometry(D2D1Geometry geometry, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => _context.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);

        /// <inheritdoc cref="D2D1RenderTarget.FillGeometry(D2D1Geometry, D2D1Brush, D2D1Brush?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void FillGeometry(D2D1Geometry geometry, D2D1Brush brush, D2D1Brush? opacityBrush = null)
            => _context.FillGeometry(geometry, brush, opacityBrush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity,
            D2D1BitmapInterpolationMode interpolationMode)
            => _context.DrawBitmap(bitmap, in destinationRectangle, opacity, interpolationMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity,
            D2D1BitmapInterpolationMode interpolationMode)
            => _context.DrawBitmap(bitmap, in destinationRectangle, in sourceRectangle, opacity, interpolationMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1.0f,
            D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear, RectF* sourceRectangle = null)
            => _context.DrawBitmap(bitmap, destinationRectangle, opacity, interpolationMode, sourceRectangle);

        ///<inheritdoc cref="D2D1RenderTarget.DrawText(char, DWriteTextFormat, in RectF, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawText(char character, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => _context.DrawText(character, textFormat, layoutRect, defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="D2D1RenderTarget.DrawText(char, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawText(char character, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => _context.DrawText(character, textFormat, layoutRect, defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="D2D1RenderTarget.DrawText(string, DWriteTextFormat, in RectF, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawText(string text, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => _context.DrawText(text, textFormat, layoutRect, defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="D2D1RenderTarget.DrawText(string, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawText(string text, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => _context.DrawText(text, textFormat, layoutRect, defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="D2D1RenderTarget.DrawText(char*, uint, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawText(char* text, uint textLength, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => _context.DrawText(text, textLength, textFormat, layoutRect, defaultFillBrush, options, measuringMode);

        /// <inheritdoc cref="D2D1RenderTarget.DrawTextLayout(PointF, DWriteTextLayout, D2D1Brush, D2D1DrawTextOptions)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawTextLayout(PointF origin, DWriteTextLayout textLayout, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
            => _context.DrawTextLayout(origin, textLayout, defaultFillBrush, options);

        /// <inheritdoc cref="D2D1DeviceContext.DrawImage(D2D1Image, D2D1InterpolationMode, D2D1CompositeMode)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawImage(D2D1Image image,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => _context.DrawImage(image, interpolationMode, compositeMode);

        /// <inheritdoc cref="D2D1DeviceContext.DrawImage(D2D1Image, in PointF, D2D1InterpolationMode, D2D1CompositeMode)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawImage(D2D1Image image, in PointF targetOffset,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => _context.DrawImage(image, targetOffset, interpolationMode, compositeMode);

        /// <inheritdoc cref="D2D1DeviceContext.DrawImage(D2D1Image, in PointF, in RectF, D2D1InterpolationMode, D2D1CompositeMode)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawImage(D2D1Image image, in PointF targetOffset, in RectF imageRectangle,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => _context.DrawImage(image, targetOffset, imageRectangle, interpolationMode, compositeMode);

        /// <inheritdoc cref="D2D1DeviceContext.DrawImage(D2D1Image, PointF*, RectF*, D2D1InterpolationMode, D2D1CompositeMode)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawImage(D2D1Image image, PointF* targetOffset, RectF* imageRectangle,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => _context.DrawImage(image, targetOffset, imageRectangle, interpolationMode, compositeMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle)
            => _context.DrawBitmap(bitmap, destinationRectangle, 1.0f, D2D1InterpolationMode.Linear);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity,
           D2D1InterpolationMode interpolationMode)
            => _context.DrawBitmap(bitmap, destinationRectangle, opacity, interpolationMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle)
            => _context.DrawBitmap(bitmap, destinationRectangle, sourceRectangle, 1.0f, D2D1InterpolationMode.Linear);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity,
            D2D1InterpolationMode interpolationMode)
            => _context.DrawBitmap(bitmap, destinationRectangle, sourceRectangle, opacity, interpolationMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, in Matrix4x4 perspectiveTransform, float opacity = 1.0f,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear)
            => _context.DrawBitmap(bitmap, destinationRectangle, sourceRectangle, perspectiveTransform, opacity, interpolationMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1.0f,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, RectF* sourceRectangle = null, Matrix4x4* perspectiveTransform = null)
            => _context.DrawBitmap(bitmap, destinationRectangle, opacity, interpolationMode, sourceRectangle, perspectiveTransform);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderingLayerToken PushLayer(in D2D1LayerParameters parameters)
            => new RenderingLayerToken(_context, parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderingLayerToken PushLayer(in D2D1LayerParametersNative parameters)
            => new RenderingLayerToken(_context, parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderingClipToken PushAxisAlignedClip(in RectF clipRect, D2D1AntialiasMode antialiasMode)
            => new RenderingClipToken(_context, in clipRect, antialiasMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderingClipToken PushPixelAlignedClip(ref RectF clipRect, D2D1AntialiasMode antialiasMode)
            => PushPixelAlignedClip(in clipRect, antialiasMode, out clipRect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderingClipToken PushPixelAlignedClip(in RectF clipRect, D2D1AntialiasMode antialiasMode, out RectF actualClipRect)
        {
            actualClipRect = GetPixelAlignedRect(in clipRect);
            if (actualClipRect.IsEmpty)
                return default;
            return new RenderingClipToken(_context, in actualClipRect, antialiasMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RegionalRenderingContext WithEmptyDirtyCollector()
            => _collector.IsEmptyInstance ? this : new RegionalRenderingContext(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RegionalRenderingContext WithAxisAlignedClip(in RectF clipRect, D2D1AntialiasMode antialiasMode)
            => new RegionalRenderingContext(_context, _collector, _pointsPerPixel, in clipRect, antialiasMode, isPixelAligned: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RegionalRenderingContext WithPixelAlignedClip(ref RectF clipRect, D2D1AntialiasMode antialiasMode)
            => WithPixelAlignedClip(in clipRect, antialiasMode, out clipRect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RegionalRenderingContext WithPixelAlignedClip(in RectF clipRect, D2D1AntialiasMode antialiasMode, out RectF actualClipRect)
        {
            actualClipRect = GetPixelAlignedRect(clipRect);
            return new RegionalRenderingContext(_context, _collector, _pointsPerPixel, in actualClipRect, antialiasMode, isPixelAligned: true);
        }

        public readonly RectF GetPixelAlignedRect(in RectF rect)
        {
            RectF sourceClipRect = _clipToken.ClipRect;
            float width = sourceClipRect.Width;
            float height = sourceClipRect.Height;
            RectF adjustedRect = new RectF(MathHelper.Min(rect.Left, width), MathHelper.Min(rect.Top, height),
                MathHelper.Min(rect.Right, width), MathHelper.Min(rect.Bottom, height));
            if (!adjustedRect.IsValid)
                return default;
            if (_isPixelAligned)
                return RenderingHelper.RoundInPixel(adjustedRect, _pointsPerPixel);
            return TranslateAreaToLocal(
                RenderingHelper.RoundInPixel(TranslateAreaToGlobal(adjustedRect), _pointsPerPixel));
        }

        public readonly RectF GetBorderRect(out float strokeWidth)
        {
            if (_isPixelAligned)
            {
                strokeWidth = RenderingHelper.GetDefaultBorderWidth(_pointsPerPixel);
                return GetBorderRectCore(RectF.FromXYWH(PointF.Empty, Size), strokeWidth);
            }
            return GetBorderRect(RectF.FromXYWH(PointF.Empty, Size), out strokeWidth);
        }

        public readonly RectF GetBorderRect(in RectF rect, out float strokeWidth)
        {
            float pointsPerPixel = _pointsPerPixel;
            strokeWidth = RenderingHelper.GetDefaultBorderWidth(pointsPerPixel);
            if (_isPixelAligned)
                return GetBorderRectCore(RenderingHelper.RoundInPixel(in rect, pointsPerPixel), strokeWidth);
            return TranslateAreaToLocal(GetBorderRectCore(
                RenderingHelper.RoundInPixel(TranslateAreaToGlobal(in rect), pointsPerPixel), strokeWidth));
        }

        private static RectF GetBorderRectCore(in RectF rect, float strokeWidth)
        {
            float unit = strokeWidth * 0.5f;
            return new RectF(rect.Left + unit, rect.Top + unit,
                rect.Right - unit, rect.Bottom - unit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasAnyDirtyArea() => _collector.HasAnyDirtyArea();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void UsePresentAllModeOnce() => _collector.UsePresentAllModeOnce();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void MarkAsDirty()
        {
            DirtyAreaCollector collector = _collector;
            if (collector.IsEmptyInstance)
                return;
            MarkAsDirtyCore(RectF.FromXYWH(PointF.Empty, Size));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void MarkAsDirty(in RectF rect)
        {
            DirtyAreaCollector collector = _collector;
            if (collector.IsEmptyInstance)
                return;
            MarkAsDirtyCore(rect);
        }

        private readonly void MarkAsDirtyCore(in RectF rect)
        {
            DirtyAreaCollector collector = _collector;
            if (collector.IsEmptyInstance)
                return;
            collector.MarkAsDirty(TranslateAreaToGlobal(rect));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly RectF TranslateAreaToGlobal(in RectF rect)
        {
            PointF offsetPoint = _offsetPoint;
            float offsetX = offsetPoint.X;
            float offsetY = offsetPoint.Y;
            return new RectF(rect.Left + offsetX, rect.Top + offsetY,
                rect.Right + offsetX, rect.Bottom + offsetY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly RectF TranslateAreaToLocal(in RectF rect)
        {
            PointF offsetPoint = _offsetPoint;
            float offsetX = offsetPoint.X;
            float offsetY = offsetPoint.Y;
            return new RectF(rect.Left - offsetX, rect.Top - offsetY,
                rect.Right - offsetX, rect.Bottom - offsetY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _context.Transform = _originalTransform;
            _clipToken.Dispose();
        }
    }
}