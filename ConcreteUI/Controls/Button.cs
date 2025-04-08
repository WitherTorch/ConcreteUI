using System;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class Button : ButtonBase, IDisposable
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "border",
            "border.hovered",
            "face",
            "face.hovered",
            "face.pressed",
            "fore",
            "fore.inactive",
        }.WithPrefix("app.button.").ToLowerAscii();

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

        private DWriteTextFormat _format;
        private DWriteTextLayout _layout;
        private string _text, _fontName;

        private bool _disposed;
        private float _fontSize;
        private long _rawUpdateFlags;

        public Button(IRenderer renderer) : base(renderer)
        {
            _fontSize = 14;
            _rawUpdateFlags = -1L;
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            if (Renderer.IsInitializingElements())
                return;
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
        }

        protected override void ApplyThemeCore(ThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            _fontName = provider.FontName;
			Update(RenderObjectUpdateFlags.FormatAndLayout);
        }

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        [Inline(InlineBehavior.Remove)]
        private DWriteTextLayout GetTextLayout()
        {
            RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
            if ((flags & RenderObjectUpdateFlags.Layout) != RenderObjectUpdateFlags.Layout)
                return _layout;
            DWriteTextFormat format;
            if ((flags & RenderObjectUpdateFlags.FormatAndLayout) == RenderObjectUpdateFlags.FormatAndLayout)
            {
                format = TextFormatUtils.CreateTextFormat(TextAlignment.MiddleCenter, _fontName, _fontSize);
                DisposeHelper.SwapDispose(ref _format, format);
            }
            else
            {
                format = _format;
            }
            string text = _text;
            DWriteTextLayout layout;
            if (string.IsNullOrEmpty(text))
                layout = null;
            else
                layout = SharedResources.DWriteFactory.CreateTextLayout(_text, format);
            DisposeHelper.SwapDispose(ref _layout, layout);
            return layout;
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            IRenderer renderer = Renderer;
            D2D1DeviceContext deviceContext = renderer.GetDeviceContext();
            float lineWidth = renderer.GetBaseLineWidth();
            Rect bounds = Bounds;
            RectF buttonBounds = GraphicsUtils.AdjustRectangleAsBorderBounds(bounds, lineWidth);

            D2D1Brush[] brushes = _brushes;
            deviceContext.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
            switch (PressState)
            {
                case ButtonPressState.Default:
                    RenderBackground(deviceContext, brushes[(int)Brush.FaceBrush]);
                    deviceContext.DrawRectangle(buttonBounds, brushes[(int)Brush.BorderBrush], lineWidth);
                    break;
                case ButtonPressState.Hovered:
                    RenderBackground(deviceContext, brushes[(int)Brush.FaceHoveredBrush]);
                    deviceContext.DrawRectangle(buttonBounds, brushes[(int)Brush.BorderHoveredBrush], lineWidth);
                    break;
                case ButtonPressState.Pressed:
                    RenderBackground(deviceContext, brushes[(int)Brush.FacePressedBrush]);
                    deviceContext.DrawRectangle(buttonBounds, brushes[(int)Brush.BorderHoveredBrush], lineWidth);
                    break;
                default:
                    break;
            }
            DWriteTextLayout layout = GetTextLayout();
            if (layout is null)
            {
                deviceContext.PopAxisAlignedClip();
                return true;
            }

            D2D1Brush brush = Enabled ? brushes[(int)Brush.TextBrush] : brushes[(int)Brush.TextDisabledBrush];
            layout.MaxWidth = bounds.Width;
            layout.MaxHeight = bounds.Height;
            deviceContext.DrawTextLayout(bounds.TopLeft, layout, brush, D2D1DrawTextOptions.Clip);
            deviceContext.PopAxisAlignedClip();
            return true;
        }

        private void DisposeCore()
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeHelper.SwapDispose(ref _format);
            DisposeHelper.SwapDispose(ref _layout);
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
