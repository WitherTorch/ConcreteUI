using System;
using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ProgressBar : UIElement, IDisposable
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "border",
            "fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private double _value, _maximium;
        private bool _disposed;

        public ProgressBar(IElementContainer parent) : base(parent, "app.progressBar")
        {
            _value = 0.0f;
            _maximium = 100.0f;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);

        protected override bool IsBackgroundOpaqueCore() => GraphicsUtils.CheckBrushIsSolid(
            UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush));

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
            SizeF renderSize = context.Size;

            double percentage = _value / _maximium;
            RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush));
            context.FillRectangle(
                new RectF(0, 0, RenderingHelper.RoundInPixel((float)(renderSize.Width * percentage), context.PixelsPerPoint.X), renderSize.Height),
                UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.ForeBrush));
            context.DrawBorder(UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BorderBrush));
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;

            if (disposing)
            {
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            }
            SequenceHelper.Clear(_brushes);
        }

        ~ProgressBar()
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
