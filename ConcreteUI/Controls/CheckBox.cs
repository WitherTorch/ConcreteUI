using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Controls
{
    public sealed partial class CheckBox : DisposableUIElementBase, IMouseInteractHandler, IMouseMoveHandler
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
        private DWriteTextLayout? _layout;

        private ButtonTriState _buttonState;
        private long _redrawTypeRaw, _rawUpdateFlags;
        private float _fontSize;
        private bool _checkState, _isPressed;

        public CheckBox(IElementContainer parent) : base(parent, "app.checkBox")
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
            UIElementHelper.ApplyThemeUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
            _fontName = provider.FontName;
            _rawUpdateFlags = -1L;
            _fontSize = UIConstants.DefaultFontSize;
            DisposeHelper.SwapDispose(ref _layout);
            Update(RedrawType.RedrawAllContent);
        }

        public override void OnSizeChanged() => Update();

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
                        D2D1Brush textBrush = UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.TextBrush);
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
            ButtonTriState buttonState = _buttonState;
            if (buttonState > ButtonTriState.Pressed)
                return;
            RectangleF renderingBounds = GetCheckBoxRenderingBounds(in context, context.Size.Height);
            if (context.HasDirtyCollector)
            {
                using RenderingClipScope scope = context.PushAxisAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased);
                RenderBackground(in context);
                context.MarkAsDirty(renderingBounds);
            }
            DrawCheckBoxUnsafe(context, _brushes, renderingBounds, _checkState, buttonState);
        }

        public static RectangleF GetCheckBoxRenderingBounds(in RegionalRenderingContext context, float itemHeight)
        {
            Vector2 pointsPerPixel = context.PixelsPerPoint;
            float borderWidth = context.DefaultBorderWidth;
            float buttonWidth = RenderingHelper.RoundInPixel(itemHeight, pointsPerPixel.Y) - borderWidth * 2;
            return new RectangleF(borderWidth, borderWidth, buttonWidth, buttonWidth);
        }

        public static void DrawCheckBox(in RegionalRenderingContext context, D2D1Brush?[] brushes, in RectangleF renderingBounds,
            bool checkState, ButtonTriState hoverState)
        {
            if (hoverState > ButtonTriState.Pressed || brushes.Length < (int)Brush._CheckBoxRenderingLast)
                return;
            DrawCheckBoxUnsafe(context, brushes, renderingBounds, checkState, hoverState);
        }

        internal static void DrawCheckBoxUnsafe(in RegionalRenderingContext context, D2D1Brush?[] brushes, in RectangleF renderingBounds,
            bool checkState, ButtonTriState hoverState)
        {
            ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
            D2D1Brush? backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef,
                MathHelper.BooleanToNativeUnsigned(checkState) * (nuint)Brush.BorderCheckedBrush + (nuint)hoverState);
            if (backBrush is null)
                return;

            using RegionalRenderingContext clipContext = context.WithPixelAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased, out RectF _);
            if (checkState)
            {
                RectF bounds = clipContext.Bounds;
                clipContext.FillRectangle(bounds, backBrush);
                D2D1Brush? markBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.MarkBrush);
                if (markBrush is null)
                    return;
                FontIconResources.Instance.DrawCheckMark(clipContext, new RectangleF(PointF.Empty, clipContext.Size), markBrush);
            }
            else
            {
                clipContext.DrawBorder(backBrush);
            }
        }

        public void OnMouseMove(in MouseEventArgs args)
        {
            ButtonTriState oldButtonState = _buttonState;
            ButtonTriState newButtonState;
            if (args.IsInSpecificSize(Size))
                newButtonState = _isPressed ? ButtonTriState.Pressed : ButtonTriState.Hovered;
            else
                newButtonState = ButtonTriState.None;
            if (oldButtonState == newButtonState)
                return;
            _buttonState = newButtonState;
            Update(RedrawType.RedrawCheckBox);
        }

        public void OnMouseDown(ref HandleableMouseEventArgs args)
        {
            if (_buttonState != ButtonTriState.Hovered || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;

            args.Handle();
            _isPressed = true;
            _buttonState = ButtonTriState.Pressed;
            Checked = !Checked;
        }

        public void OnMouseUp(in MouseEventArgs args)
        {
            if (!args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;
            _isPressed = false;

            if (_buttonState != ButtonTriState.Pressed)
                return;
            _buttonState = ButtonTriState.Hovered;
            Update(RedrawType.RedrawCheckBox);
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            }
            SequenceHelper.Clear(_brushes);
        }
    }
}
