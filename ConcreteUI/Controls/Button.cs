using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
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

        private DWriteTextLayout? _layout;
        private string? _fontName;
        private string _text;

        private bool _disposed;
        private float _fontSize;
        private long _rawUpdateFlags;

        public Button(IRenderer renderer) : base(renderer)
        {
            _fontSize = UIConstants.DefaultFontSize;
            _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
            _text = string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button WithAutoWidthCalculation(int minHeight = -1, int maxHeight = -1)
        {
            WidthCalculation = new AutoWidthCalculation(this, minHeight, maxHeight);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button WithAutoHeightCalculation(int minHeight = -1, int maxHeight = -1)
        {
            HeightCalculation = new AutoHeightCalculation(this, minHeight, maxHeight);
            return this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, (int)Brush._Last);
            _fontName = provider.FontName;
            DisposeHelper.SwapDisposeInterlocked(ref _layout);
            Update(RenderObjectUpdateFlags.Format);
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        [Inline(InlineBehavior.Remove)]
        private DWriteTextLayout? GetTextLayout(RenderObjectUpdateFlags flags)
        {
            DWriteTextLayout? layout = Interlocked.Exchange(ref _layout, null);

            if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
            {
                DWriteTextFormat? format = layout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleCenter, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
                string text = _text;
                if (StringHelper.IsNullOrEmpty(text))
                    layout = null;
                else
                    layout = SharedResources.DWriteFactory.CreateTextLayout(text, format);
                format.Dispose();
            }
            return layout;
        }

        [Inline(InlineBehavior.Remove)]
        private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
        {
            if (format is null || format.IsDisposed)
                return true;
            if ((flags & RenderObjectUpdateFlags.Format) == RenderObjectUpdateFlags.Format)
            {
                format.Dispose();
                return true;
            }
            return false;
        }

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
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
            DWriteTextLayout? layout = GetTextLayout(flags);
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
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            return true;
        }

        private void DisposeCore()
        {
            if (_disposed)
                return;
            _disposed = true;
            DisposeHelper.SwapDispose(ref _layout);
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
