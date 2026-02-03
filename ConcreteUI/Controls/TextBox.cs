using System;
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
using ConcreteUI.Input;
using ConcreteUI.Internals;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Layout;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Text;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    public sealed partial class TextBox : ScrollableElementBase, IIMEControl, IMouseInteractEvents, IMouseNotifyEvents, IKeyEvents, ICharacterEvents, ICursorPredicator
    {
        private static readonly LazyTiny<GraphemeInfo> EmptyGraphemeInfoLazy =
            new LazyTiny<GraphemeInfo>(new GraphemeInfo(string.Empty, Array.Empty<int>()));
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "back.disabled",
            "border",
            "border.focused",
            "fore",
            "fore.inactive",
            "selection.back",
            "selection.fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
        private readonly LayoutVariable?[] _autoLayoutVariableCache = new LayoutVariable?[1];
        private readonly CoreWindow _window;
        private readonly InputMethod? _ime;
        private readonly Timer _caretTimer;

        private LazyTiny<GraphemeInfo> _textGraphemeInfoLazy;
        private DWriteTextLayout? _layout, _watermarkLayout;
        private string? _fontName;
        private string _text, _watermark;
        private DWriteTextRange _compositionRange;
        private SelectionRange _selectionRange;
        private SystemCursorType? _cursorType;
        private TextAlignment _alignment;
        private long _rawUpdateFlags;
        private float _fontSize;
        private int _caretIndex, _compositionCaretIndex, _borderBrushIndex;
        private char _passwordChar;
        private bool _caretState, _focused, _multiLine, _imeEnabled;

        public TextBox(CoreWindow window) : base(window, "app.textBox")
        {
            _window = window;
            window.FocusElementChanged += Window_FocusElementChanged;
            _caretTimer = new Timer(CaretTimer_Tick, this, Timeout.Infinite, Timeout.Infinite);
            _caretState = true;
            _caretIndex = 0;
            _compositionCaretIndex = 0;
            _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
            _text = string.Empty;
            _textGraphemeInfoLazy = EmptyGraphemeInfoLazy;
            _watermark = string.Empty;
            _fontSize = UIConstants.BoxFontSize;
            _borderBrushIndex = (int)Brush.BorderBrush;
            _passwordChar = '\0';
            ScrollBarType = ScrollBarType.AutoVertial;
            SurfaceSize = new Size(int.MaxValue, 0);
            DrawWhenDisabled = true;
            StickBottom = true;
        }

        public TextBox(CoreWindow window, InputMethod? ime) : this(window)
        {
            _ime = ime;
            _imeEnabled = ime is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextBox WithAutoHeight()
        {
            HeightVariable = AutoHeightReference;
            return this;
        }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyTheme(provider, _brushes, _brushNames, ThemePrefix, (int)Brush._Last);
            _fontName = provider.FontName;
            DisposeHelper.SwapDispose(ref _layout);
            DisposeHelper.SwapDispose(ref _watermarkLayout);
            Update(RenderObjectUpdateFlags.Format);
        }

        protected override D2D1Brush GetBackBrush() => _brushes[(int)Brush.BackBrush];

        protected override D2D1Brush GetBackDisabledBrush() => _brushes[(int)Brush.BackDisabledBrush];

        protected override D2D1Brush GetBorderBrush() => _brushes[_borderBrushIndex];

        protected override void OnEnableChanged(bool enable)
        {
            base.OnEnableChanged(enable);
            InputMethod? ime = _imeEnabled ? _ime : null;
            if (ime is not null)
            {
                if (enable && _focused)
                    ime.Attach(this);
                else
                    ime.Detach(this);
            }
            Update();
        }

        protected override Rect OnContentBoundsChanging(in Rect bounds)
        {
            if (bounds.Width < UIConstants.ElementMargin || bounds.Height < UIConstants.ElementMargin)
                return default;
            return new Rect(bounds.Left + UIConstants.ElementMarginHalf, bounds.Top + UIConstants.ElementMarginHalf,
                bounds.Right - UIConstants.ElementMarginHalf, bounds.Bottom - UIConstants.ElementMarginHalf);
        }

        private static GraphemeInfo CreateGraphemeInfoForString(string str)
        {
            if (str.Length <= 0)
                return GraphemeInfo.Empty;
            return new GraphemeInfo(str, GraphemeHelper.GetGraphemeIndices(str));
        }

        private void Window_FocusElementChanged(object? sender, UIElement? element)
        {
            bool newFocus = this == element;
            if (_focused == newFocus)
                return;
            _focused = newFocus;
            if (newFocus)
            {
                _borderBrushIndex = (int)Brush.BorderFocusedBrush;
                bool enabled = Enabled;
                if (_imeEnabled && enabled)
                    _ime?.Attach(this);
            }
            else
            {
                _borderBrushIndex = (int)Brush.BorderBrush;
                _ime?.Detach(this);
                _compositionRange.Length = 0;
                _selectionRange.Length = 0;
                if (!_multiLine)
                    ViewportPoint = Point.Empty;
            }
            UpdateCaretIndex(_caretIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string FixString(string value)
        {
            if (value is null)
                return string.Empty;
            if (_multiLine)
                return value;
            return StringHelper.GetStringForFirstNonEmptyLine(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Update()
        {
            InterlockedHelper.Exchange(ref _rawUpdateFlags, (long)RenderObjectUpdateFlags.FlagsAllTrue);
            Update(ScrollableElementUpdateFlags.All);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update(ScrollableElementUpdateFlags.All);
        }

        [Inline(InlineBehavior.Remove)]
        private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
        {
            return (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);
        }

        private void GetTextLayouts(out DWriteTextLayout? layout, out DWriteTextLayout? watermarkLayout)
        {
            RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
            layout = Interlocked.Exchange(ref _layout, null);
            watermarkLayout = Interlocked.Exchange(ref _watermarkLayout, null);
            if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
            {
                DWriteTextFormat? format = layout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(_alignment, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);

                string text = _text;
                if (!StringHelper.IsNullOrEmpty(text))
                {
                    char passwordChar = PasswordChar;
                    if (passwordChar != '\0') //has password char
                    {
                        DWriteTextRange compositionRange = _compositionRange;
                        if (compositionRange.Length > 0) //has ime composition
                        {
                            if (compositionRange.StartPosition > 0)
                            {
                                text = string.Concat(new string(passwordChar, MathHelper.MakeSigned(compositionRange.StartPosition)),
                                    text.Substring(MathHelper.MakeSigned(compositionRange.StartPosition), MathHelper.MakeSigned(compositionRange.Length)),
                                    new string(passwordChar, text.Length - MathHelper.MakeSigned(compositionRange.StartPosition + compositionRange.Length)));
                            }
                        }
                        else
                        {
                            text = new string(passwordChar, text.Length);
                        }
                    }
                }
                layout = SharedResources.DWriteFactory.CreateTextLayout(text ?? string.Empty, format);
                format.Dispose();
            }
            if ((flags & RenderObjectUpdateFlags.WatermarkLayout) == RenderObjectUpdateFlags.WatermarkLayout)
            {
                DWriteTextFormat? format = watermarkLayout;
                if (CheckFormatIsNotAvailable(format, flags))
                    format = TextFormatHelper.CreateTextFormat(_alignment, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
                watermarkLayout = SharedResources.DWriteFactory.CreateTextLayout(_watermark ?? string.Empty, format);
                format.Dispose();
            }
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

        [Inline(InlineBehavior.Remove)]
        private DWriteTextLayout CreateVirtualTextLayout() => CreateVirtualTextLayout(_text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DWriteTextLayout CreateVirtualTextLayout(string text)
        {
            DWriteTextLayout result = TextFormatHelper.CreateTextLayout(text, NullSafetyHelper.ThrowIfNull(_fontName), _alignment, _fontSize);
            SetRenderingProperties(result);
            return result;
        }

        private void SetRenderingProperties(DWriteTextLayout layout)
            => SetRenderingProperties(layout, ContentBounds.Size, Renderer.GetPointsPerPixel(), _multiLine);

        [Inline(InlineBehavior.Remove)]
        private void SetRenderingProperties(DWriteTextLayout layout, SizeF size, Vector2 pointsPerPixel, bool multiLine)
        {
            if (multiLine)
                SetRenderingPropertiesForMultiLine(layout, size.Width, pointsPerPixel);
            else
                SetRenderingPropertiesForSingleLine(layout, size.Height, pointsPerPixel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetRenderingPropertiesForMultiLine(DWriteTextLayout layout, float maxWidth, Vector2 pointsPerPixel)
        {
            layout.MaxWidth = RenderingHelper.CeilingInPixel(MathHelper.Max(maxWidth, 0.0f), pointsPerPixel.X);
            layout.MaxHeight = float.PositiveInfinity;
            layout.WordWrapping = DWriteWordWrapping.EmergencyBreak;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetRenderingPropertiesForSingleLine(DWriteTextLayout layout, float maxHeight, Vector2 pointsPerPixel)
        {
            layout.MaxWidth = float.PositiveInfinity;
            layout.MaxHeight = RenderingHelper.CeilingInPixel(MathHelper.Max(maxHeight, 0.0f), pointsPerPixel.Y);
            layout.WordWrapping = DWriteWordWrapping.EmergencyBreak;
        }

        private void CalculateCurrentViewportPoint()
        {
            using DWriteTextLayout layout = CreateVirtualTextLayout();
            Rect bounds = ContentBounds;
            DWriteTextRange compositionRange = _compositionRange;
            bool inComposition = compositionRange.Length > 0;
            int visualCaretIndex = _caretIndex;
            if (inComposition)
                visualCaretIndex += _compositionCaretIndex;

            DWriteHitTestMetrics metrics = layout.HitTestTextPosition(MathHelper.MakeUnsigned(visualCaretIndex), false, out float caretX, out float caretY);

            //取得視角點
            PointF viewportPoint = ViewportPoint;
            float viewportX = caretX - viewportPoint.X;
            float viewportY = caretY - viewportPoint.Y;
            #region 伸縮機制
            float edgeX = bounds.Width;
            float edgeY = bounds.Height;
            #region X方向伸縮
            if (viewportX < 0)
                viewportPoint.X += viewportX;
            else if (viewportX > edgeX)
                viewportPoint.X += viewportX - edgeX;
            else if (viewportX < edgeX && viewportPoint.X > 0 && visualCaretIndex == _text.Length)
                viewportPoint.X = MathHelper.Max(viewportPoint.X + viewportX - edgeX, 0);
            #endregion
            #region Y方向伸縮
            if (viewportY < 0)
                viewportPoint.Y += viewportY;
            else if (viewportY + metrics.Height > edgeY)
                viewportPoint.Y += viewportY - edgeY + metrics.Height;
            else if (viewportY < edgeY && viewportPoint.Y < 0)
                viewportPoint.Y = MathHelper.Min(viewportPoint.Y - viewportY + edgeY, 0);
            #endregion
            #endregion
            ViewportPoint = new Point(MathI.Round(viewportPoint.X), MathI.Round(viewportPoint.Y));
        }

        protected override bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
        {
            D2D1Brush[] brushes = _brushes;
            SizeF renderSize = context.Size;
            bool focused = _focused;

            if (context.HasDirtyCollector)
            {
                RenderBackground(context, backBrush);
                context.MarkAsDirty();
            }

            GetTextLayouts(out DWriteTextLayout? layout, out DWriteTextLayout? watermarkLayout);

            if (layout is null || (layout.DetermineMinWidth() <= 0.0f && (!_multiLine || !SequenceHelper.Contains(_text, '\n'))))
            {
                if (watermarkLayout is null)
                    return true;
                SetRenderingProperties(watermarkLayout, renderSize, context.PointsPerPixel, _multiLine);
                //文字為空，繪製浮水印
                using (ClearTypeScope scope = ClearTypeScope.Enter(Renderer, context, backBrush))
                    RenderLayoutCore(context, brushes[(int)Brush.ForeInactiveBrush], watermarkLayout, PointF.Empty);
                if (focused)
                    DrawCaret(context, watermarkLayout, PointF.Empty, 0);
                if (layout is not null)
                    DisposeHelper.NullSwapOrDispose(ref _layout, layout);
                DisposeHelper.NullSwapOrDispose(ref _watermarkLayout, watermarkLayout);
                return true;
            }

            SetRenderingProperties(layout, renderSize, context.PointsPerPixel, _multiLine);
            using (ClearTypeScope token = ClearTypeScope.Enter(Renderer, context, backBrush))
                RenderLayout(context, focused, layout, RectF.FromXYWH(PointF.Empty, renderSize));
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            if (watermarkLayout is not null)
                DisposeHelper.NullSwapOrDispose(ref _watermarkLayout, watermarkLayout);

            return true;
        }

        private void RenderLayout(in RegionalRenderingContext context, bool focused, DWriteTextLayout layout, in RectF layoutRect)
        {
            D2D1Brush[] brushes = _brushes;
            PointF viewportPoint = ViewportPoint;
            //輸出處理 (IME 作用中範圍提示、選取範圍提示、取得視角點等等)
            DWriteTextRange compositionRange = _compositionRange;
            bool inComposition = compositionRange.Length > 0;
            //取得視角點
            PointF layoutPoint = new PointF(layoutRect.Left - viewportPoint.X, layoutRect.Top - viewportPoint.Y);
            //IME 作用中範圍提示
            if (inComposition)
                layout.SetUnderline(true, compositionRange);
            //選取範圍提示
            SelectionRange selectionRange = _selectionRange;
            if (selectionRange.Length > 0)
            {
                DWriteTextRange textRange = selectionRange.ToTextRange();
                D2D1Brush selectionBackBrush = brushes[(int)Brush.SelectionBackBrush];
                D2D1Brush selectionForeBrush = brushes[(int)Brush.SelectionForeBrush];
                layout.SetDrawingEffect(selectionForeBrush, textRange);
                DWriteHitTestMetrics[] metricsArray = layout.HitTestTextRange(textRange.StartPosition,
                   MathHelper.MakeUnsigned(selectionRange.Length), 0, 0);
                int length = metricsArray is null ? 0 : metricsArray.Length;
                if (length > 0)
                {
                    Vector2 pointsPerPixel = Renderer.GetPointsPerPixel();
                    for (int i = 0; i < length; i++)
                    {
                        DWriteHitTestMetrics rangeMetrics = metricsArray![i];
                        RectF selectionBounds = RenderingHelper.RoundInPixel(RectF.FromXYWH(
                            layoutPoint.X + rangeMetrics.Left, layoutPoint.Y + rangeMetrics.Top, rangeMetrics.Width, rangeMetrics.Height),
                            pointsPerPixel);
                        context.FillRectangle(selectionBounds, selectionBackBrush);
                    }
                }
            }
            //繪製文字
            RenderLayoutCore(context, Enabled ? brushes[(int)Brush.ForeBrush] : brushes[(int)Brush.ForeInactiveBrush], layout, layoutPoint);
            if (selectionRange.Length > 0)
            {
                DWriteTextRange textRange = selectionRange.ToTextRange();
                layout.SetDrawingEffect(null, textRange);
            }
            //繪製閃爍的文字輸入條
            if (focused)
            {
                int visualCaretIndex = _caretIndex;
                if (inComposition)
                    visualCaretIndex += _compositionCaretIndex;
                DrawCaret(context, layout, layoutPoint, visualCaretIndex);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private static void RenderLayoutCore(in RegionalRenderingContext context, D2D1Brush foreBrush, DWriteTextLayout layout, PointF point)
        {
            //繪製文字
            context.DrawTextLayout(point, layout, foreBrush, D2D1DrawTextOptions.EnableColorFont);
        }

        private unsafe void DrawCaret(in RegionalRenderingContext context, DWriteTextLayout layout, PointF layoutPoint, int caretIndex)
        {
            if (!_caretState)
                return;
            DWriteHitTestMetrics rangeMetrics;
            uint returnCount;
            int hr = layout.TryHitTestTextRange(MathHelper.MakeUnsigned(caretIndex), 0, 0, 0, &rangeMetrics, 1, &returnCount);
            if (ConcreteSettings.UseDebugMode)
                ThrowHelper.ThrowExceptionForHR(hr);
            else if (hr < 0)
                return;
            if (returnCount < 1)
                return;
            Vector2 pointsPerPixel = Renderer.GetPointsPerPixel();
            RectF selectionBounds = RenderingHelper.RoundInPixel(RectF.FromXYWH(
                layoutPoint.X + rangeMetrics.Left, layoutPoint.Y + rangeMetrics.Top, 1.0f, rangeMetrics.Height),
                pointsPerPixel);
            context.FillRectangle(selectionBounds, _brushes[(int)Brush.ForeBrush]);
        }

        private void UpdateTextAndCaretIndex(string text, int caretIndex, bool checkCaretIndex = true)
        {
            text = FixString(text);

            if (!IsRenderedOnce)
            {
                _text = text;
                InterlockedHelper.Exchange(ref _textGraphemeInfoLazy, new LazyTiny<GraphemeInfo>(
                    () => CreateGraphemeInfoForString(text)));
                return;
            }

            TextChangingEventHandler? changingHandler = TextChanging;
            if (changingHandler is not null)
            {
                TextChangingEventArgs args = new TextChangingEventArgs(text);
                changingHandler.Invoke(this, args);
                if (args is not null)
                {
                    if (args.IsCanceled)
                        return;
                    if (args.IsEdited)
                        text = FixString(args.Text);
                }
            }

            int length = text.Length;
            GraphemeInfo graphemeInfo = CreateGraphemeInfoForString(text);
            _text = text;
            InterlockedHelper.Exchange(ref _textGraphemeInfoLazy, new LazyTiny<GraphemeInfo>(graphemeInfo));
            if (checkCaretIndex)
            {
                if (caretIndex <= 0)
                    caretIndex = 0;
                else if (caretIndex >= length)
                    caretIndex = length;
                else
                    caretIndex = AdjustCaretIndexCore(caretIndex, length, graphemeInfo.GraphemeIndices, takeGreaterIfNotExists: false);
            }

            if (_multiLine)
            {
                float contentWidth = ContentBounds.Width;
                if (contentWidth > 0f)
                {
                    using DWriteTextLayout layout = CreateVirtualTextLayout(text);
                    layout.MaxWidth = contentWidth;
                    SurfaceSize = new Size(0, MathI.Ceiling(layout.GetMetrics().Height));
                }
                else
                {
                    SurfaceSize = Size.Empty;
                }
            }
            _selectionRange.Length = 0;

            UpdateCaretIndex(caretIndex, RenderObjectUpdateFlags.Layout);
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Normal Key Controls

        public void OnKeyDown(ref KeyInteractEventArgs args)
        {
            if (!_focused || !Enabled)
                return;
            KeyInteractEventHandler? eventHandler = KeyDown;
            if (eventHandler is not null)
            {
                eventHandler.Invoke(this, ref args);
                if (args.Handled)
                    return;
            }
            if (Keys.IsAltPressed())
                return;
            bool isCtrlPressed = Keys.IsControlPressed();
            bool isShiftPressed = Keys.IsShiftPressed();
            bool justCtrlPressed = isCtrlPressed && !isShiftPressed;
            VirtualKey keyCode = args.Key;
            switch (keyCode)
            {
                case VirtualKey.X when justCtrlPressed: // Ctrl + X
                    Cut();
                    break;
                case VirtualKey.C when justCtrlPressed: // Ctrl + C
                    Copy();
                    break;
                case VirtualKey.V when justCtrlPressed: // Ctrl + V
                    Paste();
                    break;
                case VirtualKey.A when justCtrlPressed: // Ctrl + A
                    SelectAll();
                    break;
                case VirtualKey.Delete:
                    DeleteOne();
                    break;
                case VirtualKey.LeftArrow when isCtrlPressed:
                case VirtualKey.Home:
                    MoveToStart(isShiftPressed);
                    break;
                case VirtualKey.RightArrow when isCtrlPressed:
                case VirtualKey.End:
                    MoveToEnd(isShiftPressed);
                    break;
                case VirtualKey.LeftArrow:
                    MoveLeft(isShiftPressed);
                    break;
                case VirtualKey.RightArrow:
                    MoveRight(isShiftPressed);
                    break;
                case VirtualKey.UpArrow:
                    MoveUp();
                    break;
                case VirtualKey.DownArrow:
                    MoveDown();
                    break;
                case VirtualKey.Enter:
                    NextLine();
                    break;
            }
        }

        public void OnKeyUp(ref KeyInteractEventArgs args)
        {
            if (!_focused || !Enabled)
                return;
            KeyUp?.Invoke(this, ref args);
            if (args.Handled)
                return;
            switch (args.Key)
            {
                case VirtualKey.Applications:
                case VirtualKey.F10 when Keys.IsKeyPressed(VirtualKey.Shift):
                    MouseNotifyEventHandler? eventHandlers = RequestContextMenu;
                    if (eventHandlers is not null)
                    {
                        Point location = ContentBounds.Location;
                        Point viewportPoint = ViewportPoint;
                        Point layoutPoint = new Point(location.X - viewportPoint.X, location.Y - viewportPoint.Y);
                        using (DWriteTextLayout layout = CreateVirtualTextLayout())
                        {
                            layout.HitTestTextPosition(MathHelper.MakeUnsigned(_caretIndex), isTrailingHit: true, out float pointX, out float pointY);
                            location = new Point(layoutPoint.X + MathI.Round(pointX), layoutPoint.Y + MathI.Round(pointY));
                        }
                        eventHandlers.Invoke(this, new MouseNotifyEventArgs(location, MouseButtons.RightButton));
                    }
                    break;
            }
        }
        #endregion

        #region IME Support
        void IIMEControl.StartIMEComposition()
        { }

        [LocalsInit(false)]
        unsafe void IIMEControl.OnIMEComposition(string str, IMECompositionFlags flags, int cursorPosition)
        {
            string text = _text;
            int caretIndex = _caretIndex;
            RemoveSelectionCore(ref text, ref caretIndex, ReferenceHelper.Exchange(ref _selectionRange, default));

            if (cursorPosition < 0)
                cursorPosition = str.Length;

            DWriteTextRange compositionRange = _compositionRange;
            if (compositionRange.Length <= 0)
            {
                compositionRange.StartPosition = MathHelper.MakeUnsigned(caretIndex);
                compositionRange.Length = MathHelper.MakeUnsigned(str.Length);
                _compositionCaretIndex = cursorPosition;
                _compositionRange = compositionRange;
                text = text.Insert(MathHelper.MakeSigned(compositionRange.StartPosition), str);
            }
            else
            {
                int startPos = MathHelper.MakeSigned(compositionRange.StartPosition);
                int length = MathHelper.MakeSigned(compositionRange.Length);
                compositionRange.Length = MathHelper.MakeUnsigned(str.Length);
                _compositionCaretIndex = cursorPosition;
                _compositionRange = compositionRange;

                int capacity = text.Length - length + MathHelper.Max(str.Length, length);
                using StringBuilderTiny builder = new StringBuilderTiny();
                if (Limits.UseStackallocStringBuilder && capacity <= Limits.MaxStackallocChars)
                {
                    char* buffer = stackalloc char[capacity];
                    builder.SetStartPointer(buffer, capacity);
                }
                else
                {
                    builder.EnsureCapacity(capacity);
                }
                builder.Append(text);
                builder.Remove(startPos, length);
                builder.Insert(startPos, str);
                text = builder.ToString();
            }
            UpdateTextAndCaretIndex(text, caretIndex);
        }

        [LocalsInit(false)]
        unsafe void IIMEControl.OnIMECompositionResult(string str, IMECompositionFlags flags)
        {
            string text = _text;
            int caretIndex = _caretIndex;

            DWriteTextRange compositionRange = _compositionRange;
            if (compositionRange.Length == 0)
            {
                int length = str.Length;
                RemoveSelectionCore(ref text, ref caretIndex, ReferenceHelper.Exchange(ref _selectionRange, default));
                text = text.Insert(caretIndex, str);
                caretIndex += length;
            }
            else
            {
                _compositionRange = default;
                _compositionCaretIndex = 0;
                int startPos = MathHelper.MakeSigned(compositionRange.StartPosition);
                int length = MathHelper.MakeSigned(compositionRange.Length);
                int capacity = text.Length - length + MathHelper.Max(str.Length, length);
                using StringBuilderTiny builder = new StringBuilderTiny();
                if (Limits.UseStackallocStringBuilder && capacity <= Limits.MaxStackallocChars)
                {
                    char* buffer = stackalloc char[capacity];
                    builder.SetStartPointer(buffer, capacity);
                }
                else
                {
                    builder.EnsureCapacity(capacity);
                }
                builder.Append(text);
                builder.Remove(startPos, length);
                builder.Insert(startPos, str);
                text = builder.ToString();
                caretIndex += length;
            }
            UpdateTextAndCaretIndex(text, caretIndex);
        }

        void IIMEControl.EndIMEComposition()
        {
            uint length = ReferenceHelper.Exchange(ref _compositionRange, default).Length;
            if (length <= 0)
                return;
            UpdateCaretIndex(_caretIndex + MathHelper.MakeSigned(length));
        }

        [LocalsInit(false)]
        unsafe void ICharacterEvents.OnCharacterInput(ref CharacterInteractEventArgs args)
        {
            if (!_focused || !Enabled)
                return;
            char character = args.Character;
            if (character < '\u0020' && character != '\b')
                return;
            args.Handle();
            string text = _text;
            int caretIndex = _caretIndex;
            switch (character)
            {
                case '\b': // Backspace
                    {
                        SelectionRange selectionRange = ReferenceHelper.Exchange(ref _selectionRange, default);
                        if (selectionRange.Length > 0)
                            RemoveSelectionCore(ref text, ref caretIndex, selectionRange);
                        else
                        {
                            if (caretIndex <= 0)
                                return;
                            int newCaretIndex = AdjustCaretIndex(text, caretIndex - 1, takeGreaterIfNotExists: false);
                            text = text.Remove(newCaretIndex, caretIndex - newCaretIndex);
                            caretIndex = newCaretIndex;
                        }
                    }
                    break;
                case '\u007f': //DEL (Ctrl + Backspace)
                    {
                        SelectionRange selectionRange = ReferenceHelper.Exchange(ref _selectionRange, default);
                        if (selectionRange.Length > 0)
                            RemoveSelectionCore(ref text, ref caretIndex, selectionRange);
                        else
                        {
                            if (caretIndex <= 0)
                                return;
                            int newCaretIndex = MathHelper.Min(caretIndex, text.Length) - 1;
                            for (; newCaretIndex >= 0; newCaretIndex--)
                            {
                                if (!char.IsLetterOrDigit(text[newCaretIndex]))
                                    break;
                            }
                            if (newCaretIndex < 0)
                            {
                                caretIndex = 0;
                                text = string.Empty;
                            }
                            else
                            {
                                newCaretIndex = AdjustCaretIndex(text, newCaretIndex, takeGreaterIfNotExists: false);
                                text = text.Remove(newCaretIndex, caretIndex - newCaretIndex);
                                caretIndex = newCaretIndex;
                            }
                        }
                    }
                    break;
                default:
                    {
                        SelectionRange selectionRange = ReferenceHelper.Exchange(ref _selectionRange, default);
                        if (selectionRange.Length > 0)
                            RemoveSelectionCore(ref text, ref caretIndex, selectionRange);

                        int length = text.Length;
                        int newLength = length + 1;
                        string newText = StringHelper.AllocateRawString(newLength);
                        fixed (char* ptr = newText)
                        {
                            using StringBuilderTiny builder = new StringBuilderTiny();
                            builder.SetStartPointer(ptr, newLength);
                            builder.Append(text, 0, caretIndex);
                            builder.Append(character);
                            builder.Append(text, caretIndex, length - caretIndex);
                        }
                        text = newText;
                        caretIndex++;
                    }
                    break;
            }
            UpdateTextAndCaretIndex(text, caretIndex, checkCaretIndex: false);
        }
        #endregion

        #region TextBox Functions
        public void Cut()
        {
            string text = _selectionRange.Length <= 0 ? string.Empty : _text.Substring(MathHelper.MakeSigned(_selectionRange.ToTextRange().StartPosition), _selectionRange.Length);
            RemoveSelection();
            Clipboard.SetText(text);
        }

        public void Copy()
        {
            SelectionRange selectionRange = _selectionRange;
            string text;
            if (selectionRange.Length > 0)
            {
                text = _text.Substring(MathHelper.MakeSigned(selectionRange.ToTextRange().StartPosition), selectionRange.Length);
                if (string.IsNullOrEmpty(text))
                    text = string.Empty;
            }
            else
            {
                text = string.Empty;
            }
            Clipboard.SetText(text);
        }

        public void Paste()
        {
            string text = _text;
            int caretIndex = _caretIndex;
            RemoveSelectionCore(ref text, ref caretIndex, ReferenceHelper.Exchange(ref _selectionRange, default));
            try
            {
                string? str = Clipboard.GetText();
                if (!StringHelper.IsNullOrEmpty(str))
                {
                    text = text.Insert(caretIndex, str);
                    caretIndex += str.Length;
                }
            }
            finally
            {
                UpdateTextAndCaretIndex(text, caretIndex);
            }
        }

        public void SelectAll()
        {
            int length = _text.Length;
            _selectionRange = new SelectionRange(0, length);
            UpdateCaretIndex(length);
        }

        public void RemoveSelection()
        {
            string text = _text;
            int caretIndex = _caretIndex;
            RemoveSelectionCore(ref text, ref caretIndex, ReferenceHelper.Exchange(ref _selectionRange, default));
            UpdateTextAndCaretIndex(text, caretIndex);
        }

        private void RemoveSelectionCore(ref string str, ref int caretIndex, in SelectionRange selectionRange)
        {
            int selectionLength = selectionRange.Length;
            if (selectionLength <= 0)
                return;
            DWriteTextRange range = selectionRange.ToTextRange();
            caretIndex = MathHelper.MakeSigned(range.StartPosition);
            str = str.Remove(caretIndex, selectionLength);
        }

        [Inline(InlineBehavior.Remove)]
        private void DeleteOne()
        {
            string text = _text;
            if (StringHelper.IsNullOrEmpty(text))
                return;
            int caretIndex = _caretIndex;
            SelectionRange selectionRange = ReferenceHelper.Exchange(ref _selectionRange, default);
            if (selectionRange.Length > 0)
                RemoveSelectionCore(ref text, ref caretIndex, in selectionRange);
            else
            {
                int length = text.Length;

                if (caretIndex >= length)
                {
                    int newCaretIndex = AdjustCaretIndex(text, caretIndex - 1, takeGreaterIfNotExists: false);
                    text = text.Remove(newCaretIndex, caretIndex - newCaretIndex);
                    caretIndex = newCaretIndex;
                }
                else
                {
                    int indexForRemoval = AdjustCaretIndex(text, caretIndex + 1, takeGreaterIfNotExists: true);
                    text = text.Remove(caretIndex, indexForRemoval - caretIndex);
                }
            }
            UpdateTextAndCaretIndex(text, caretIndex, checkCaretIndex: false);
        }

        private int AdjustCaretIndex(int caretIndex, bool takeGreaterIfNotExists)
        {
            if (caretIndex <= 0)
                return 0;

            GraphemeInfo graphemeInfo = InterlockedHelper.Read(ref _textGraphemeInfoLazy).Value;
            int length = graphemeInfo.Original.Length;
            if (caretIndex >= length)
                return length;
            return AdjustCaretIndexCore(caretIndex, length, graphemeInfo.GraphemeIndices, takeGreaterIfNotExists);
        }

        private int AdjustCaretIndex(string str, int caretIndex, bool takeGreaterIfNotExists)
        {
            if (caretIndex <= 0)
                return 0;

            int length = str.Length;
            if (caretIndex >= length)
                return length;

            GraphemeInfo graphemeInfo = InterlockedHelper.Read(ref _textGraphemeInfoLazy).Value;
            int[] indices = ReferenceEquals(str, graphemeInfo.Original) ? graphemeInfo.GraphemeIndices : GraphemeHelper.GetGraphemeIndices(str);
            return AdjustCaretIndexCore(caretIndex, length, indices, takeGreaterIfNotExists);
        }

        private int AdjustCaretIndexCore(int caretIndex, int originalStringLength, int[] graphemeIndices, bool takeGreaterIfNotExists)
        {
            int index = Array.BinarySearch(graphemeIndices, caretIndex);
            if (index >= 0)
                return caretIndex;
            index = ~index;
            int indicesCount = graphemeIndices.Length;
            if (index < indicesCount)
                return takeGreaterIfNotExists ? graphemeIndices[index] : graphemeIndices[MathHelper.Max(index, 1) - 1];
            return takeGreaterIfNotExists ? originalStringLength : graphemeIndices[indicesCount - 1];
        }

        private void AddCaretIndex(int increment) => UpdateCaretIndex(_caretIndex + increment);

        private void UpdateCaretIndex(int caretIndex, RenderObjectUpdateFlags updateFlags = RenderObjectUpdateFlags.None)
        {
            _caretIndex = caretIndex;
            _caretState = true;
            if (Enabled && _focused)
                _caretTimer.Change(500, 500);
            else
                _caretTimer.Change(Timeout.Infinite, Timeout.Infinite);
            CalculateCurrentViewportPoint();
            Update(updateFlags);
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveToStart(bool isSelectionMode)
        {
            bool isInComposition = _compositionRange.Length > 0;
            SelectionRange selectionRange = _selectionRange;
            if (isSelectionMode)
            {
                if (selectionRange.Length == 0)
                {
                    if (isInComposition)
                        selectionRange.EndPosition = _caretIndex + _compositionCaretIndex;
                    else
                        selectionRange.EndPosition = _caretIndex;
                }
                else
                {
                    selectionRange.EndPosition = MathHelper.Max(selectionRange.StartPosition, selectionRange.EndPosition);
                }
            }
            if (isInComposition)
            {
                _compositionCaretIndex = 0;
                if (isSelectionMode)
                    selectionRange.StartPosition = _caretIndex;
                _selectionRange = selectionRange;
                Update();
            }
            else
            {
                _selectionRange = selectionRange;
                UpdateCaretIndex(0);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveToEnd(bool isSelectionMode)
        {
            int caretIndex = _caretIndex;
            int compositionLength = MathHelper.MakeSigned(_compositionRange.Length);
            int textLength = _text.Length;
            bool isInComposition = compositionLength > 0;
            SelectionRange newSelectionRange = new SelectionRange(textLength, textLength);
            if (isSelectionMode)
            {
                SelectionRange selectionRange = _selectionRange;
                if (selectionRange.Length == 0)
                {
                    if (isInComposition)
                        newSelectionRange.StartPosition = caretIndex + _compositionCaretIndex;
                    else
                        newSelectionRange.StartPosition = caretIndex;
                }
                else
                {
                    newSelectionRange.StartPosition = MathHelper.Min(selectionRange.StartPosition, selectionRange.EndPosition);
                }
            }
            if (isInComposition)
            {
                _compositionCaretIndex = compositionLength;
                if (isSelectionMode)
                    newSelectionRange.EndPosition = caretIndex + compositionLength;
                _selectionRange = newSelectionRange;
                Update();
            }
            else
            {
                _selectionRange = newSelectionRange;
                UpdateCaretIndex(textLength);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveLeft(bool isSelectionMode)
        {
            if (_compositionRange.Length > 0)
            {
                _compositionCaretIndex = MathHelper.Max(_compositionCaretIndex - 1, 0);
                _selectionRange.Length = 0;
                Update();
                return;
            }
            int caretIndex = _caretIndex;
            bool selection = false;
            if (isSelectionMode)
            {
                selection = true;
                if (_selectionRange.Length <= 0)
                {
                    _selectionRange.StartPosition = caretIndex;
                }
            }
            else
            {
                _selectionRange.Length = 0;
            }
            caretIndex = AdjustCaretIndex(caretIndex - 1, takeGreaterIfNotExists: false);
            if (selection)
                _selectionRange.EndPosition = caretIndex;
            UpdateCaretIndex(caretIndex);
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveRight(bool isSelectionMode)
        {
            int compositionLength = MathHelper.MakeSigned(_compositionRange.Length);
            if (compositionLength > 0)
            {
                _compositionCaretIndex = MathHelper.Min(_compositionCaretIndex + 1, compositionLength);
                _selectionRange.Length = 0;
                Update();
                return;
            }
            int caretIndex = _caretIndex;
            bool selection = false;
            if (isSelectionMode)
            {
                selection = true;
                if (_selectionRange.Length <= 0)
                {
                    _selectionRange.StartPosition = caretIndex;
                }
            }
            else
            {
                _selectionRange.Length = 0;
            }
            caretIndex = AdjustCaretIndex(caretIndex + 1, takeGreaterIfNotExists: true);
            if (selection)
                _selectionRange.EndPosition = caretIndex;
            UpdateCaretIndex(caretIndex);
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveUp()
        {
            if (!MultiLine)
                return;
            int caretIndex = _caretIndex;
            if (caretIndex <= 0)
                return;

            string text = _text;
            using DWriteTextLayout layout = CreateVirtualTextLayout(text);
            layout.HitTestTextPosition(MathHelper.MakeUnsigned(caretIndex), false, out float pointX, out float pointY);

            pointY -= 5;
            if (pointY < 0)
                return;

            int pos = MathHelper.MakeSigned(layout.HitTestPoint(pointX, pointY, out bool isTrailingHit, out bool isInside).TextPosition);
            if (isTrailingHit)
                pos = AdjustCaretIndex(text, pos + 1, takeGreaterIfNotExists: true);
            UpdateCaretIndex(pos);
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveDown()
        {
            if (!MultiLine)
                return;
            string text = _text;
            int caretIndex = _caretIndex;
            int textLength = text.Length;
            if (caretIndex >= textLength)
                return;
            using DWriteTextLayout layout = CreateVirtualTextLayout(text);
            DWriteHitTestMetrics metrics = layout.HitTestTextPosition(MathHelper.MakeUnsigned(caretIndex), false, out float pointX, out float pointY);
            pointY += metrics.Height + 5;
            if (pointY < 0)
                return;
            int pos = MathHelper.MakeSigned(layout.HitTestPoint(pointX, pointY, out bool isTrailingHit, out bool isInside).TextPosition);
            if (isTrailingHit)
                pos = AdjustCaretIndex(text, pos + 1, takeGreaterIfNotExists: true);
            UpdateCaretIndex(pos);
        }

        [Inline(InlineBehavior.Remove)]
        private void NextLine()
        {
            if (!MultiLine)
                return;
            string newLine = Environment.NewLine;
            int caretIndex = _caretIndex;
            UpdateTextAndCaretIndex(_text.Insert(caretIndex, newLine), caretIndex + newLine.Length);
        }
        #endregion

        private static void CaretTimer_Tick(object? state)
        {
            if (state is not TextBox _this)
                return;
            _this._caretState = !_this._caretState;
            _this.Update();
        }

        private int GetCaretIndexFromPoint(PointF point, out bool isInside)
        {
            PointF viewportPoint = ViewportPoint;
            float viewportLeft = MathF.Floor(Location.X + UIConstants.ElementMarginHalf) - viewportPoint.X;
            float viewportTop = MathF.Floor(Location.Y + UIConstants.ElementMarginHalf) - viewportPoint.Y;
            string text = _text;
            using DWriteTextLayout layout = CreateVirtualTextLayout(text);
            int result = MathHelper.MakeSigned(layout.HitTestPoint(point.X - viewportLeft, point.Y - viewportTop, out bool isTrailingHit, out isInside).TextPosition);
            if (isTrailingHit)
                result = AdjustCaretIndex(text, result + 1, takeGreaterIfNotExists: true);
            return result;
        }

        #region Mouse Events Handling

        int clicks = 0;
        long lastClickedTime = long.MinValue;
        bool _drag = false;
        SelectionRange previousSelectionRange;

        public void OnMouseDown(in MouseNotifyEventArgs args)
        {
            _window.ClearFocusElement(this);
        }

        public override void OnMouseDown(ref MouseInteractEventArgs args)
        {
            base.OnMouseDown(ref args);
            if (args.Handled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton) || !Enabled)
            {
                _drag = false;
                if (_selectionRange.Length > 0)
                {
                    _selectionRange.Length = 0;
                    Update();
                }
                return;
            }
            _window.ChangeFocusElement(this);
            if (!ContentBounds.Contains(args.Location))
            {
                _drag = false;
                return;
            }
            _drag = true;
            long currentClickedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long lastClickedTime = Interlocked.Exchange(ref this.lastClickedTime, currentClickedTime);
            int clicks;
            if (lastClickedTime > long.MinValue && currentClickedTime - lastClickedTime <= SystemParameters.DoubleClickTime)
            {
                clicks = ++this.clicks;
                if (clicks < 2)
                    this.clicks = clicks = 2;
            }
            else
            {
                this.clicks = clicks = 1;
            }
            int caretIndex = GetCaretIndexFromPoint(args.Location, out bool isInside);
            switch (clicks)
            {
                case 1:
                    previousSelectionRange = _selectionRange = new SelectionRange(caretIndex, caretIndex);
                    break;
                default:
                    string text = _text;
                    int textLength = text.Length;
                    if (textLength > 0)
                    {
                        switch ((clicks - 1) % 2)
                        {
                            case 0:
                                previousSelectionRange = _selectionRange = new SelectionRange(0, textLength);
                                break;
                            case 1:
                                {
                                    if (caretIndex >= textLength)
                                        caretIndex = textLength - 1;
                                    if (text[caretIndex] == ' ')
                                    {
                                        int selectionStart = -1, selectionEnd = -1;
                                        int index = caretIndex - 1;
                                        do
                                        {
                                            int searchingIndex = text.LastIndexOf(' ', index);
                                            if (searchingIndex < index)
                                            {
                                                selectionStart = index + 1;
                                                break;
                                            }
                                            else
                                                index--;
                                        } while (index >= 0);
                                        index = caretIndex + 1;
                                        do
                                        {
                                            int searchingIndex = StringHelper.IndexOf(text, ' ', index);
                                            if (searchingIndex > index || searchingIndex == -1)
                                            {
                                                selectionEnd = index;
                                                break;
                                            }
                                            else
                                                index++;
                                        } while (index < textLength);
                                        if (selectionStart < 0) selectionStart = 0;
                                        if (selectionEnd < 0) selectionEnd = textLength;
                                        previousSelectionRange = _selectionRange = new SelectionRange(selectionStart, selectionEnd);
                                        caretIndex = selectionEnd;
                                    }
                                    else
                                    {
                                        int selectionStart = text.LastIndexOf(' ', caretIndex) + 1;
                                        int selectionEnd = StringHelper.IndexOf(text, ' ', caretIndex);
                                        if (selectionStart < 0) selectionStart = 0;
                                        if (selectionEnd < 0) selectionEnd = text.Length;
                                        previousSelectionRange = _selectionRange = new SelectionRange(selectionStart, selectionEnd);
                                        caretIndex = selectionEnd;
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
            if (!isInside || _caretIndex != caretIndex)
                UpdateCaretIndex(caretIndex);
        }

        bool isEnter = false;
        public override void OnMouseMove(in MouseNotifyEventArgs args)
        {
            base.OnMouseMove(args);
            if (ContentBounds.Contains(args.Location))
            {
                if (!isEnter)
                {
                    _cursorType = SystemCursorType.IBeam;
                    isEnter = true;
                }
            }
            else
            {
                if (isEnter)
                {
                    _cursorType = null;
                    isEnter = false;
                }
            }
            if (_drag)
            {
                string text = _text;
                if (StringHelper.IsNullOrEmpty(text))
                    return;
                PointF location = args.Location;
                if (!_multiLine)
                {
                    using DWriteTextLayout layout = CreateVirtualTextLayout(text);
                    location.Y = Location.Y + 3 + layout.GetMetrics().Top;
                }
                int newCaretIndex = GetCaretIndexFromPoint(location, out _);
                if (_caretIndex != newCaretIndex)
                {
                    int previousSelectionStart = previousSelectionRange.StartPosition;
                    int previousSelectionEnd = previousSelectionRange.EndPosition;
                    if (newCaretIndex < previousSelectionStart)
                        _selectionRange.StartPosition = newCaretIndex;
                    else if (newCaretIndex > previousSelectionEnd)
                        _selectionRange.EndPosition = newCaretIndex;
                    UpdateCaretIndex(newCaretIndex);
                }
            }
        }

        public override void OnMouseUp(in MouseNotifyEventArgs args)
        {
            base.OnMouseUp(args);
            _drag = false;
            if (!Enabled || !args.Buttons.HasFlagOptimized(MouseButtons.RightButton))
                return;
            MouseNotifyEventHandler? eventHandler = RequestContextMenu;
            if (eventHandler is null || !ContentBounds.Contains(args.Location))
                return;
            eventHandler.Invoke(this, in args);
        }
        #endregion

        #region Disposing
        protected override void DisposeCore(bool disposing)
        {
            base.DisposeCore(disposing);
            if (disposing)
            {
                if (_imeEnabled && _focused)
                    _ime?.Detach(this);
                _caretTimer.Dispose();
                DisposeHelper.SwapDispose(ref _layout);
                DisposeHelper.SwapDispose(ref _watermarkLayout);
                DisposeHelper.DisposeAll(_brushes);
            }
            SequenceHelper.Clear(_brushes);
        }
        #endregion

        public Rect GetInputArea() => ContentBounds;

        private sealed record class GraphemeInfo(
            string Original,
            int[] GraphemeIndices
            )
        {
            public static readonly GraphemeInfo Empty = new GraphemeInfo(string.Empty, Array.Empty<int>());
        }
    }
}
