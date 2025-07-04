using System;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class FontIconButton : ButtonBase, IDisposable
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "face",
            "face.hovered",
            "face.pressed"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private FontIcon? _icon;
        private bool _disposed;

        public FontIconButton(IRenderer renderer) : base(renderer, "app.fontIconButton")
        {
            _icon = null;
            _disposed = false;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int) Brush._Last);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            RenderBackground(context);
            FontIcon? icon = Interlocked.Exchange(ref _icon, null);
            if (icon is null)
                return true;
            D2D1Brush brush;
            switch (PressState)
            {
                case ButtonPressState.Default:
                    brush = _brushes[(int)Brush.ButtonBrush];
                    break;
                case ButtonPressState.Hovered:
                    brush = _brushes[(int)Brush.ButtonHoveredBrush];
                    break;
                case ButtonPressState.Pressed:
                    brush = _brushes[(int)Brush.ButtonPressedBrush];
                    break;
                default:
                    return true;
            }
            icon.Render(context, Bounds, brush);
            DisposeHelper.NullSwapOrDispose(ref _icon, icon);
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _icon);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        ~FontIconButton()
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
