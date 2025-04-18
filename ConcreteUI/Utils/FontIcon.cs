﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils
{
    public sealed class FontIcon : IDisposable
    {
        private readonly DWriteTextLayout _layout;
        private readonly SizeF _size;

        private bool _disposed;

        public SizeF Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }

        public FontIcon(string fontName, uint unicodeValue, SizeF size)
        {
            _layout = CreateLayoutFitInSize(fontName, unicodeValue, size);
            _size = size;
        }

        private static DWriteTextLayout CreateLayoutFitInSize(string fontName, uint unicodeValue, SizeF size)
        {
            string text = StringHelper.GetStringFromUtf32Character(unicodeValue);
            DWriteFactory factory = SharedResources.DWriteFactory;
            float targetHeight = size.Height;
            float targetWidth = size.Width;
            float fontSize = targetHeight;
            do
            {
                DWriteTextFormat format = factory.CreateTextFormat(fontName, fontSize);
                DWriteTextLayout layout = factory.CreateTextLayout(text, format);
                format.Dispose();
                DWriteTextMetrics metrics = layout.GetMetrics();
                float realHeight = metrics.Height;
                if (realHeight > targetHeight)
                {
                    layout.Dispose();
                    fontSize -= targetHeight - realHeight;
                    continue;
                }
                float realWidth = metrics.Width;
                if (realWidth > targetWidth)
                {
                    layout.Dispose();
                    fontSize -= targetWidth - realWidth;
                    continue;
                }
                layout.MaxWidth = size.Width;
                layout.MaxHeight = size.Height;
                return layout;
            }
            while (fontSize > 0f);
            throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(D2D1DeviceContext context, PointF location, D2D1Brush brush)
            => context.DrawTextLayout(location, _layout, brush, D2D1DrawTextOptions.Clip);

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
