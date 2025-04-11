using System;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class ProgressBar : UIElement
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "border",
            "fore"
        }.WithPrefix("app.progressBar.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private float _value, _maximium;

        public ProgressBar(IRenderer renderer) : base(renderer)
        {
            _value = 0.0f;
            _maximium = 100.0f;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider) 
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);

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
    }
}
