using System.Drawing;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class FontIconButton : ButtonBase
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "face",
            "face.hovered",
            "face.pressed"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private FontIcon? _icon;

        public FontIconButton(IRenderer renderer) : base(renderer, "app.fontIconButton")
        {
            _icon = null;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            RenderBackground(context);
            FontIcon? icon = Interlocked.Exchange(ref _icon, null);
            if (icon is null)
                return true;
            D2D1Brush brush;
            switch (PressState)
            {
                case ButtonTriState.None:
                    brush = _brushes[(int)Brush.ButtonBrush];
                    break;
                case ButtonTriState.Hovered:
                    brush = _brushes[(int)Brush.ButtonHoveredBrush];
                    break;
                case ButtonTriState.Pressed:
                    brush = _brushes[(int)Brush.ButtonPressedBrush];
                    break;
                default:
                    return true;
            }
            icon.Render(context, new RectangleF(PointF.Empty, context.Size), brush);
            DisposeHelper.NullSwapOrDispose(ref _icon, icon);
            return true;
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _icon);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
