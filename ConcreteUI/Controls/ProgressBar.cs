using System;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ProgressBar : UIElement, IDisposable
    {
        private static readonly string[] BrushNamesTemplate = new string[(int)Brush._Last]
        {
            "back",
            "border",
            "fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly string[] _brushNames = new string[(int)Brush._Last];

        private float _value, _maximium;
        private bool _disposed;

        public ProgressBar(IRenderer renderer) : base(renderer, "app.progressBar")
        {
            _value = 0.0f;
            _maximium = 100.0f;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);

        protected override void OnThemePrefixChanged(string prefix)
            => UIElementHelper.CopyStringArrayAndAppendDottedPrefix(BrushNamesTemplate, _brushNames, (int)Brush._Last, prefix);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            Rect bounds = Bounds;
            float lineWidth = Renderer.GetBaseLineWidth();
            D2D1Brush[] brushes = _brushes;
            context.FillRectangle((RectF)bounds, brushes[(int)Brush.BackBrush]);
            context.FillRectangle(new RectF(bounds.Left, bounds.Top, MathF.Floor(bounds.Left + bounds.Width * Value / Maximium), bounds.Bottom), brushes[(int)Brush.ForeBrush]);
            context.DrawRectangle(GraphicsUtils.AdjustRectangleAsBorderBounds(bounds, lineWidth), brushes[(int)Brush.BorderBrush], lineWidth);
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
