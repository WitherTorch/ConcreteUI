using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public sealed partial class Button : ButtonBase
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
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly LayoutVariable?[] _autoLayoutVariableCache = new LayoutVariable?[2];

        private DWriteTextLayout? _layout;
        private string? _fontName;
        private string _text;

        private float _fontSize;
        private long _rawUpdateFlags;

        public Button(IRenderer renderer) : base(renderer, "app.button")
        {
            _fontSize = UIConstants.BoxFontSize;
            _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
            _text = string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button WithAutoWidth()
        {
            WidthVariable = AutoWidthReference;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button WithAutoHeight()
        {
            HeightVariable = AutoHeightReference;
            return this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
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

        protected override bool IsBackgroundOpaqueCore()
        {
            D2D1Brush[] brushes = _brushes;
            D2D1Brush brush = PressState switch
            {
                ButtonTriState.Hovered => brushes[(int)Brush.FaceHoveredBrush],
                ButtonTriState.Pressed => brushes[(int)Brush.FacePressedBrush],
                _ => brushes[(int)Brush.FaceBrush],
            };
            return GraphicsUtils.CheckBrushIsSolid(brush);
        }

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
            D2D1Brush[] brushes = _brushes;
            D2D1Brush borderBrush;
            switch (PressState)
            {
                case ButtonTriState.Hovered:
                    RenderBackground(in context, brushes[(int)Brush.FaceHoveredBrush]);
                    borderBrush = brushes[(int)Brush.BorderHoveredBrush];
                    break;
                case ButtonTriState.Pressed:
                    RenderBackground(in context, brushes[(int)Brush.FacePressedBrush]);
                    borderBrush = brushes[(int)Brush.BorderHoveredBrush];
                    break;
                default:
                    RenderBackground(in context, brushes[(int)Brush.FaceBrush]);
                    borderBrush = brushes[(int)Brush.BorderBrush];
                    break;
            }
            DWriteTextLayout? layout = GetTextLayout(flags);
            if (layout is not null)
            {
                SizeF renderSize = context.Size;
                D2D1Brush brush = Enabled ? brushes[(int)Brush.TextBrush] : brushes[(int)Brush.TextDisabledBrush];
                layout.MaxWidth = renderSize.Width;
                layout.MaxHeight = renderSize.Height;
                context.DrawTextLayout(PointF.Empty, layout, brush, D2D1DrawTextOptions.Clip);
                DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            }
            context.DrawBorder(borderBrush);
            return true;
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.SwapDispose(ref _layout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
