using System;
using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

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

        public ProgressBar(IRenderer renderer) : base(renderer, "app.progressBar")
        {
            _value = 0.0f;
            _maximium = 100.0f;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            D2D1Brush[] brushes = _brushes;
            SizeF renderSize = context.Size;

            double percentage = _value / _maximium;
            RenderBackground(context, brushes[(int)Brush.BackBrush]);
            context.FillRectangle(
                new RectF(0, 0, RenderingHelper.RoundInPixel((float)(renderSize.Width * percentage), context.PointsPerPixel.X), renderSize.Height),
                brushes[(int)Brush.ForeBrush]);
            context.DrawBorder(brushes[(int)Brush.BorderBrush]);
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;

            if (disposing)
            {
                DisposeHelper.DisposeAll(_brushes);
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
