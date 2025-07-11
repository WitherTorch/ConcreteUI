using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
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
    public sealed partial class CheckBox : UIElement, IDisposable, IMouseEvents
    {
        public event EventHandler? CheckedChanged;

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
        private D2D1Resource? _checkSign;

        private ButtonTriState _buttonState;
        private long _redrawTypeRaw, _rawUpdateFlags;
        private float _fontSize;
        private bool _checkState, _disposed;

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

        public override void Render(DirtyAreaCollector collector) => Render(collector, markDirty: false);

        protected override bool RenderCore(DirtyAreaCollector collector)
        {
            RedrawType redrawType = GetRedrawTypeAndReset();
            if (collector.IsEmptyInstance) //Force redraw
                redrawType = RedrawType.RedrawAllContent;
            else if (redrawType == RedrawType.NoRedraw)
                return true;
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            Rectangle bounds = Bounds;
            float lineWidth = Renderer.GetBaseLineWidth();
            switch (redrawType)
            {
                case RedrawType.RedrawAllContent:
                    RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
                    D2D1Brush textBrush = _brushes[(int)Brush.TextBrush];
                    context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
                    RenderBackground(context);
                    DrawCheckBox(context, null, (RectF)bounds, lineWidth);
                    DWriteTextLayout? layout = GetTextLayout(flags);
                    if (layout is not null)
                    {
                        PointF textLoc = new PointF(bounds.X + bounds.Height + lineWidth * 2.0f, bounds.Top);
                        if (bounds.Right > textLoc.X && bounds.Bottom > textLoc.Y)
                        {
                            layout.MaxWidth = bounds.Right - textLoc.X;
                            layout.MaxHeight = bounds.Bottom - textLoc.Y;
                            context.DrawTextLayout(textLoc, layout, textBrush);
                        }
                        DisposeHelper.NullSwapOrDispose(ref _layout, layout);
                    }
                    context.PopAxisAlignedClip();
                    collector.MarkAsDirty(bounds);
                    break;
                case RedrawType.RedrawCheckBox:
                    DrawCheckBox(context, collector, (RectF)bounds, lineWidth);
                    break;
            }
            return true;
        }

        private void DrawCheckBox(D2D1DeviceContext context, DirtyAreaCollector? collector, in RectF bounds, float lineWidth)
        {
            if (_strokeStyle is null)
            {
                context.AntialiasMode = D2D1AntialiasMode.PerPrimitive;
                _strokeStyle = context.GetFactory()!.CreateStrokeStyle(new D2D1StrokeStyleProperties() { DashCap = D2D1CapStyle.Round, StartCap = D2D1CapStyle.Round, EndCap = D2D1CapStyle.Round });
                context.AntialiasMode = D2D1AntialiasMode.Aliased;
            }
            bool checkState = _checkState;
            D2D1Brush[] brushes = _brushes;
            D2D1Brush? backBrush = null;
            if (checkState)
            {
                switch (_buttonState)
                {
                    case ButtonTriState.None:
                        backBrush = brushes[(int)Brush.BorderCheckedBrush];
                        break;
                    case ButtonTriState.Hovered:
                        backBrush = brushes[(int)Brush.BorderHoveredCheckedBrush];
                        break;
                    case ButtonTriState.Pressed:
                        backBrush = brushes[(int)Brush.BorderPressedCheckedBrush];
                        break;
                }
            }
            else
            {
                switch (_buttonState)
                {
                    case ButtonTriState.None:
                        backBrush = brushes[(int)Brush.BorderBrush];
                        break;
                    case ButtonTriState.Hovered:
                        backBrush = brushes[(int)Brush.BorderHoveredBrush];
                        break;
                    case ButtonTriState.Pressed:
                        backBrush = brushes[(int)Brush.BorderPressedBrush];
                        break;
                }
            }
            if (backBrush is null)
                return;
            RectF renderingBounds = RectF.FromXYWH(bounds.X, bounds.Y, bounds.Height, bounds.Height);
            context.PushAxisAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased);
            if (checkState)
            {
                RenderBackground(context, backBrush);
                context.Transform = new Matrix3x2() { Translation = new Vector2(renderingBounds.X, renderingBounds.Y), M11 = 1f, M22 = 1f };
                D2D1StrokeStyle strokeStyle = _strokeStyle;
                D2D1Resource checkSign = UIElementHelper.GetOrCreateCheckSign(ref _checkSign, context, strokeStyle, renderingBounds);
                if (checkSign is D2D1GeometryRealization geometryRealization && context is D2D1DeviceContext1 context1)
                    context1.DrawGeometryRealization(geometryRealization, brushes[(int)Brush.MarkBrush]);
                else if (checkSign is D2D1Geometry geometry)
                    context.DrawGeometry(geometry, brushes[(int)Brush.MarkBrush], 2.0f, strokeStyle);
                context.Transform = Matrix3x2.Identity;
            }
            else
            {
                RenderBackground(context);
                context.DrawRectangle(GraphicsUtils.AdjustRectangleFAsBorderBounds(renderingBounds, lineWidth), backBrush, lineWidth);
            }
            context.PopAxisAlignedClip();
            collector?.MarkAsDirty(renderingBounds);
        }

        public void OnMouseMove(in MouseInteractEventArgs args)
        {
            ButtonTriState oldButtonState = _buttonState;
            ButtonTriState newButtonState = ButtonTriState.None;
            if (Bounds.Contains(args.Location))
                newButtonState = ButtonTriState.Hovered;
            if (oldButtonState == newButtonState)
                return;
            if (oldButtonState != ButtonTriState.Pressed)
            {
                _buttonState = newButtonState;
                Update(RedrawType.RedrawCheckBox);
            }
        }

        public void OnMouseDown(in MouseInteractEventArgs args)
        {
            if (_buttonState == ButtonTriState.Hovered)
            {
                _buttonState = ButtonTriState.Pressed;
                Checked = !Checked;
            }
        }

        public void OnMouseUp(in MouseInteractEventArgs args)
        {
            if (_buttonState == ButtonTriState.Pressed)
            {
                if (Bounds.Contains(args.Location))
                {
                    _buttonState = ButtonTriState.Hovered;
                }
                else
                {
                    _buttonState = ButtonTriState.None;
                }
                Update(RedrawType.RedrawCheckBox);
            }
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
