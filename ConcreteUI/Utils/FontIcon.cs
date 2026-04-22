using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Utils
{
    public sealed class FontIcon : IDisposable
    {
        private readonly DWriteTextLayout _layout;
        private readonly SizeF _size, _offset;

        private bool _disposed;

        public SizeF Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }

        public FontIcon(string fontName, uint unicodeValue, SizeF size) : this(fontName, DWriteFontWeight.Normal, DWriteFontStyle.Normal, unicodeValue, size) { }

        public FontIcon(string fontName, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle, uint unicodeValue, SizeF size)
        {
            _layout = CreateLayoutFitInSize(fontName, fontWeight, fontStyle, unicodeValue, size, out _offset);
            _size = size;
        }

        private static DWriteTextLayout CreateLayoutFitInSize(string fontName, DWriteFontWeight fontWeight, DWriteFontStyle fontStyle,
            uint unicodeValue, SizeF size, out SizeF offset)
        {
            string text = StringHelper.GetStringFromUtf32Character(unicodeValue);
            DWriteFactory factory = SharedResources.DWriteFactory;
            float targetHeight = size.Height;
            float targetWidth = size.Width;
            float fontSize = targetHeight;
            do
            {
                DWriteTextFormat format = factory.CreateTextFormat(fontName, fontSize, fontWeight, fontStyle);
                format.ParagraphAlignment = DWriteParagraphAlignment.Near;
                format.TextAlignment = DWriteTextAlignment.Leading;
                format.WordWrapping = DWriteWordWrapping.NoWrap;
                DWriteTextLayout layout = factory.CreateTextLayout(text, format);
                format.Dispose();
                DWriteTextMetrics metrics = layout.GetMetrics();
                float height = metrics.Top + metrics.Height;
                float width = metrics.Left + metrics.Width;
                layout.MaxHeight = height;
                layout.MaxWidth = width;
                RectF predictedBounds = layout.GetOverhangMetrics();
                height += predictedBounds.Top + predictedBounds.Bottom;
                if (height > targetHeight)
                {
                    layout.Dispose();
                    fontSize -= height - targetHeight;
                    continue;
                }
                width += predictedBounds.Left + predictedBounds.Right;
                if (width > targetWidth)
                {
                    layout.Dispose();
                    fontSize -= width - targetWidth;
                    continue;
                }
                layout.ParagraphAlignment = DWriteParagraphAlignment.Center;
                layout.TextAlignment = DWriteTextAlignment.Center;
                layout.MaxWidth = size.Width;
                layout.MaxHeight = size.Height;
                offset = new SizeF(predictedBounds.Width * 0.5f, predictedBounds.Height * 0.5f);
                return layout;
            }
            while (fontSize > 0f);
            throw new InvalidOperationException();
        }

        public void Render(D2D1DeviceContext context, PointF location, D2D1Brush brush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
        {
            using ClearTypeScope scope = ClearTypeScope.Enter(context, enable: false);

            DWriteTextLayout layout = _layout;

            lock (layout)
            {
                (layout.MaxWidth, layout.MaxHeight) = _size;
                context.DrawTextLayout(PointF.Subtract(location, _offset), layout, brush, options);
            }
        }

        public void Render(D2D1DeviceContext context, in RectangleF rect, D2D1Brush brush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
        {
            using ClearTypeScope scope = ClearTypeScope.Enter(context, enable: false);

            DWriteTextLayout layout = _layout;

            lock (layout)
            {
                layout.MaxHeight = rect.Height;
                layout.MaxWidth = rect.Width;
                context.DrawTextLayout(PointF.Subtract(rect.Location, _offset), layout, brush, options);
            }
        }

        public void Render(in RegionalRenderingContext context, PointF location, D2D1Brush brush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
        {
            using ClearTypeScope scope = ClearTypeScope.Enter(in context, enable: false);

            DWriteTextLayout layout = _layout;

            lock (layout)
            {
                (layout.MaxWidth, layout.MaxHeight) = _size;
                context.DrawTextLayout(PointF.Subtract(location, _offset), layout, brush, options);
            }
        }

        public void Render(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
        {
            using ClearTypeScope scope = ClearTypeScope.Enter(in context, enable: false);

            DWriteTextLayout layout = _layout;

            lock (layout)
            {
                layout.MaxHeight = rect.Height;
                layout.MaxWidth = rect.Width;
                context.DrawTextLayout(PointF.Subtract(rect.Location, _offset), layout, brush, options);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
                _layout.Dispose();
        }

        ~FontIcon()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
