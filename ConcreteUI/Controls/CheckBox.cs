using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    delegate void RenderBackgroundDelegate<TElement>(TElement element, in RegionalRenderingContext context) where TElement : UIElement;

    public sealed partial class CheckBox : UIElement, IDisposable, IMouseInteractEvents
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "border",
            "border.hovered" ,
            "border.pressed",
            "border.checked" ,
            "border.hovered.checked" ,
            "border.pressed.checked",
            "mark",
            "fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly LayoutVariable?[] _autoLayoutVariableCache = new LayoutVariable?[2];

        private string? _fontName;
        private string _text;
        private D2D1StrokeStyle? _strokeStyle;
        private DWriteTextLayout? _layout;
        private D2D1PathGeometry? _checkSign;

        private ButtonTriState _buttonState;
        private long _redrawTypeRaw, _rawUpdateFlags;
        private float _fontSize;
        private bool _checkState, _isPressed, _disposed;

        public CheckBox(IRenderer renderer) : base(renderer, "app.checkBox")
        {
            _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
            _redrawTypeRaw = (long)RedrawType.RedrawAllContent;
            _fontSize = UIConstants.DefaultFontSize;
            _checkState = false;
            _text = string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CheckBox WithAutoWidth()
        {
            WidthVariable = AutoWidthReference;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CheckBox WithAutoHeight()
        {
            HeightVariable = AutoHeightReference;
            return this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            _fontName = provider.FontName;
            _rawUpdateFlags = -1L;
            _fontSize = UIConstants.DefaultFontSize;
            DisposeHelper.SwapDispose(ref _layout);
            Update(RedrawType.RedrawAllContent);
        }

        public override void OnSizeChanged()
        {
            DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Update() => Update(RedrawType.RedrawAllContent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update(RedrawType type)
        {
            if (type == RedrawType.NoRedraw)
                return;
            InterlockedHelper.Or(ref _redrawTypeRaw, (long)type);
            UpdateCore();
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private RedrawType GetRedrawTypeAndReset()
            => (RedrawType)Interlocked.Exchange(ref _redrawTypeRaw, (long)RedrawType.NoRedraw);

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
            => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

        public override bool NeedRefresh()
        {
            if (_redrawTypeRaw > (long)RedrawType.NoRedraw)
                return true;
            return Interlocked.Read(ref _redrawTypeRaw) > (long)RedrawType.NoRedraw;
        }

        private DWriteTextLayout? GetTextLayout(RenderObjectUpdateFlags flags)
        {
            DWriteTextLayout? layout = Interlocked.Exchange(ref _layout, null);

            if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
            {
                DWriteTextFormat? format = layout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleLeft, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
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

        public override void Render(in RegionalRenderingContext context) => Render(in context, markDirty: false);

        protected override bool RenderCore(in RegionalRenderingContext context)
        {
            RedrawType redrawType = GetRedrawTypeAndReset();
            if (!context.HasDirtyCollector) //Force redraw
                redrawType = RedrawType.RedrawAllContent;
            else if (redrawType == RedrawType.NoRedraw)
                return true;

            SizeF renderSize = context.Size;
            switch (redrawType)
            {
                case RedrawType.RedrawAllContent:
                    {
                        RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
                        D2D1Brush textBrush = _brushes[(int)Brush.TextBrush];
                        RenderBackground(context);
                        DrawCheckBox(context.WithEmptyDirtyCollector());
                        DWriteTextLayout? layout = GetTextLayout(flags);
                        if (layout is not null)
                        {
                            PointF textLocation = new PointF(renderSize.Height + 3.0f, 0);
                            if (textLocation.X < renderSize.Width && renderSize.Height > 0)
                            {
                                layout.MaxWidth = renderSize.Width - textLocation.X;
                                layout.MaxHeight = renderSize.Height;
                                context.DrawTextLayout(textLocation, layout, textBrush);
                            }
                            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
                        }
                        context.MarkAsDirty();
                    }
                    break;
                case RedrawType.RedrawCheckBox:
                    DrawCheckBox(context);
                    break;
            }
            return true;
        }

        private void DrawCheckBox(in RegionalRenderingContext context)
        {
            RectF renderingBounds = GetCheckBoxRenderingBounds(in context, context.Size.Height);
            using RenderingClipToken token = context.PushAxisAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased);
            if (context.HasDirtyCollector)
            {
                RenderBackground(in context);
                context.MarkAsDirty(renderingBounds);
            }
            DrawCheckBox(context, _brushes, ref _checkSign, ref _strokeStyle, renderingBounds, _checkState, _buttonState);
        }

        public static RectF GetCheckBoxRenderingBounds(in RegionalRenderingContext context, float itemHeight)
        {
            float pointsPerPixel = context.PointsPerPixel;
            float borderWidth = context.DefaultBorderWidth;
            float buttonWidth = RenderingHelper.RoundInPixel(itemHeight, pointsPerPixel) - borderWidth * 2;
            return RectF.FromXYWH(borderWidth, borderWidth, buttonWidth, buttonWidth);
        }

        public static void DrawCheckBox(in RegionalRenderingContext context, D2D1Brush?[] brushes,
            ref D2D1PathGeometry? checkSign, ref D2D1StrokeStyle? strokeStyle, in RectF renderingBounds,
            bool checkState, ButtonTriState hoverState)
        {
            if (hoverState > ButtonTriState.Pressed)
                return;
            D2D1Brush? backBrush;
            if (checkState)
                backBrush = UnsafeHelper.AddTypedOffset(ref brushes[(int)Brush.BorderCheckedBrush], (nuint)hoverState);
            else
                backBrush = UnsafeHelper.AddTypedOffset(ref brushes[(int)Brush.BorderBrush], (nuint)hoverState);
            if (backBrush is null)
                return;

            if (checkState)
            {
                context.FillRectangle(renderingBounds, backBrush);
                D2D1DeviceContext deviceContext = context.DeviceContext;
                strokeStyle ??= deviceContext.GetFactory()!.CreateStrokeStyle(new D2D1StrokeStyleProperties()
                    {
                        DashCap = D2D1CapStyle.Round,
                        StartCap = D2D1CapStyle.Round,
                        EndCap = D2D1CapStyle.Round
                    });
                checkSign ??= CreateCheckSign(deviceContext, renderingBounds.Height);
                (D2D1AntialiasMode antialiasModeBefore, deviceContext.AntialiasMode) = (deviceContext.AntialiasMode, D2D1AntialiasMode.PerPrimitive);
                try
                {
                    D2D1Brush? markBrush = brushes[(int)Brush.MarkBrush];
                    if (markBrush is null)
                        return;
                    context.DrawGeometry(checkSign, markBrush, 2.0f, strokeStyle);
                }
                finally
                {
                    deviceContext.AntialiasMode = antialiasModeBefore;
                }
            }
            else
            {
                float strokeWidth = RenderingHelper.GetDefaultBorderWidth(context.PointsPerPixel);
                context.DrawRectangle(GetBorderRectCore(renderingBounds, strokeWidth), backBrush, strokeWidth);
            }
        }

        private static RectF GetBorderRectCore(in RectF rect, float strokeWidth)
        {
            float unit = strokeWidth * 0.5f;
            return new RectF(rect.Left + unit, rect.Top + unit,
                rect.Right - unit, rect.Bottom - unit);
        }

        private static D2D1PathGeometry CreateCheckSign(D2D1DeviceContext context, float iconHeight)
        {
            D2D1PathGeometry geometry = context.GetFactory()!.CreatePathGeometry();
            using (D2D1GeometrySink sink = geometry.Open())
            {
                float unit = iconHeight / 7f;
                sink.BeginFigure(new PointF(1 + unit, 1 + unit * 3), D2D1FigureBegin.Filled);
                sink.AddLine(new PointF(1 + unit * 3, 1 + unit * 5));
                sink.AddLine(new PointF(1 + unit * 6, 1 + unit * 2));
                sink.EndFigure(D2D1FigureEnd.Open);
                sink.Close();
            }
            return geometry;
        }

        public void OnMouseMove(in MouseNotifyEventArgs args)
        {
            ButtonTriState oldButtonState = _buttonState;
            ButtonTriState newButtonState;
            if (Bounds.Contains(args.Location))
                newButtonState = _isPressed ? ButtonTriState.Pressed : ButtonTriState.Hovered;
            else
                newButtonState = ButtonTriState.None;
            if (oldButtonState == newButtonState)
                return;
            _buttonState = newButtonState;
            Update(RedrawType.RedrawCheckBox);
        }

        public void OnMouseDown(ref MouseInteractEventArgs args)
        {
            if (_buttonState != ButtonTriState.Hovered || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            args.Handle();
            _isPressed = true;
            _buttonState = ButtonTriState.Pressed;
            Checked = !Checked;
        }

        public void OnMouseUp(in MouseNotifyEventArgs args)
        {
            if (!args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;
            _isPressed = false;

            if (_buttonState != ButtonTriState.Pressed)
                return;
            _buttonState = ButtonTriState.Hovered;
            Update(RedrawType.RedrawCheckBox);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _checkSign);
                DisposeHelper.SwapDisposeInterlocked(ref _strokeStyle);
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }

        ~CheckBox()
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
