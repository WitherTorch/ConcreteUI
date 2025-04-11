using System;
using System.Drawing;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class TextButton : ButtonBase, IDisposable
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "face",
            "face.hovered",
            "face.pressed"
        }.WithPrefix("app.textButton.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private DWriteTextLayout? _layout;
        private string? _fontName;
        private string _text;
        private float _fontSize, _autoFontSizeScale;
        private bool _autoFontSize;
        private bool _disposed;

        public TextButton(IRenderer renderer) : base(renderer)
        {
            _autoFontSize = true;
            _autoFontSizeScale = UIConstants.SquareRoot1Over2_Single;
            _layout = null;
            _text = string.Empty;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
            => UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);

        private DWriteTextLayout? UpdateTextLayout()
        {
            string text = _text;
            DWriteFactory writeFactory = SharedResources.DWriteFactory;
            DWriteTextLayout textLayout;
            Rectangle bounds = Bounds;
            float fontSize = _fontSize;
            string? fontName = _fontName;
            if (StringHelper.IsNullOrEmpty(fontName))
                return null;
            if (_autoFontSize)
            {
                float targetTextWidth;
                if (fontSize > 0)
                {
                    targetTextWidth = 0;
                }
                else
                {
                    targetTextWidth = MathHelper.Min(bounds.Width, bounds.Height) * _autoFontSizeScale;
                }
                DWriteTextFormat textFormat;
                float actualTextWidth = 0;
                do
                {
                    if (actualTextWidth > 0)
                    {
                        fontSize -= actualTextWidth - targetTextWidth;
                    }
                    else
                    {
                        fontSize = targetTextWidth;
                    }
                    textFormat = writeFactory.CreateTextFormat(fontName, fontSize);
                    textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
                    textFormat.TextAlignment = DWriteTextAlignment.Center;
                    textLayout = writeFactory.CreateTextLayout(_text, textFormat);
                    actualTextWidth = textLayout.GetMetrics().Width;
                    textFormat.Dispose();
                }
                while (targetTextWidth > 0 && actualTextWidth > targetTextWidth);
                textLayout.MaxHeight = bounds.Height;
                textLayout.MaxWidth = bounds.Width;
                _fontSize = fontSize;
            }
            else
            {
                using DWriteTextFormat textFormat = writeFactory.CreateTextFormat(fontName, fontSize);
                textFormat.ParagraphAlignment = DWriteParagraphAlignment.Center;
                textFormat.TextAlignment = DWriteTextAlignment.Center;
                textLayout = writeFactory.CreateTextLayout(_text, textFormat, bounds.Width, bounds.Height);
            }
            DisposeHelper.SwapDisposeInterlocked(ref _layout, textLayout);
            return textLayout;
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            DWriteTextLayout? layout = _layout;
            layout ??= UpdateTextLayout();
            RenderBackground(context);
            if (layout is not null)
            {
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
                context.DrawTextLayout(GraphicsUtils.AdjustRectangleF(Bounds).Location, layout, brush, D2D1DrawTextOptions.Clip);
            }
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            _layout?.Dispose();
        }

        ~TextButton()
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
