using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Input;
using ConcreteUI.Internals;
using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Layout;
using ConcreteUI.Native;
using ConcreteUI.Theme;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Text;
using WitherTorch.Common.Windows.Structures;

using Cursor = System.Windows.Forms.Cursor;
using Keys = System.Windows.Forms.Keys;

namespace ConcreteUI.Controls
{
    public sealed partial class TextBox : ScrollableElementBase, IIMEControl, IGlobalMouseEvents, IKeyEvents, ICharacterEvents, ICursorPredicator
    {
        private static readonly char[] LineSeparators = ['\r', '\n'];
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

        private Cursor? _cursor;
        private DWriteTextLayout? _layout, _watermarkLayout;
        private string? _fontName;
        private string _text, _watermark;
        private DWriteTextRange compositionRange;
        private SelectionRange selectionRange;
        private TextAlignment _alignment;
        private long _rawUpdateFlags;
        private float _fontSize;
        private int _caretIndex, _compositionCaretIndex;
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
            _watermark = string.Empty;
            _fontSize = UIConstants.BoxFontSize;
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

        protected override D2D1Brush GetBorderBrush() => _focused ? _brushes[(int)Brush.BorderFocusedBrush] : _brushes[(int)Brush.BorderBrush];

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

        private void Window_FocusElementChanged(object? sender, UIElement? element)
        {
            bool newFocus = this == element;
            if (_focused == newFocus)
                return;
            _focused = newFocus;
            if (newFocus)
            {
                bool enabled = Enabled;
                _caretState = true;
                _caretTimer.Change(500, 500);
                if (_imeEnabled && enabled)
                {
                    _ime?.Attach(this);
                }
            }
            else
            {
                _caretTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _ime?.Detach(this);
                compositionRange.Length = 0;
                selectionRange.Length = 0;
                if (!_multiLine)
                    ViewportPoint = Point.Empty;
            }
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private string FixString(string value)
        {
            if (value is null)
                return string.Empty;
            if (_multiLine)
                return value;
            char[] separators = LineSeparators;
            if (!value.Contains(separators))
                return value;
            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        }

        [Inline(InlineBehavior.Remove)]
        private void Update(RenderObjectUpdateFlags flags)
        {
            InterlockedHelper.Or(ref _rawUpdateFlags, (long)flags);
            Update();
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
                        DWriteTextRange compositionRange = this.compositionRange;
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
        private DWriteTextLayout CreateVirtualTextLayout()
        {
            DWriteTextLayout result = TextFormatHelper.CreateTextLayout(_text ?? string.Empty, NullSafetyHelper.ThrowIfNull(_fontName), _alignment, _fontSize);
            SetRenderingProperties(result);
            return result;
        }

        private void SetRenderingProperties(DWriteTextLayout layout)
            => SetRenderingProperties(layout, ContentBounds, _multiLine);

        [Inline(InlineBehavior.Remove)]
        private static void SetRenderingProperties(DWriteTextLayout layout, in Rect bounds, bool multiLine)
        {
            if (multiLine)
                SetRenderingPropertiesForMultiLine(layout, bounds.Width);
            else
                SetRenderingPropertiesForSingleLine(layout, bounds.Height);
        }

        [Inline(InlineBehavior.Remove)]
        private static void SetRenderingPropertiesForMultiLine(DWriteTextLayout layout, int maxWidth)
        {
            layout.MaxWidth = MathHelper.MakeUnsigned(maxWidth);
            layout.MaxHeight = float.PositiveInfinity;
            layout.WordWrapping = DWriteWordWrapping.EmergencyBreak;
        }

        [Inline(InlineBehavior.Remove)]
        private static void SetRenderingPropertiesForSingleLine(DWriteTextLayout layout, int maxHeight)
        {
            layout.MaxWidth = float.PositiveInfinity;
            layout.MaxHeight = MathHelper.MakeUnsigned(maxHeight);
            layout.WordWrapping = DWriteWordWrapping.EmergencyBreak;
        }

        private void CalculateCurrentViewportPoint()
        {
            using DWriteTextLayout layout = CreateVirtualTextLayout();
            Rect bounds = ContentBounds;
            DWriteTextRange compositionRange = this.compositionRange;
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
            {
                viewportPoint.X += viewportX;
            }
            else if (viewportX > edgeX)
            {
                viewportPoint.X += viewportX - edgeX;
            }
            else if (viewportX < edgeX && viewportPoint.X > 0 && visualCaretIndex == Text.Length)
            {
                viewportPoint.X = MathHelper.Max(viewportPoint.X + viewportX - edgeX, 0);
            }
            #endregion
            #region Y方向伸縮
            if (viewportY < 0)
            {
                viewportPoint.Y += viewportY;
            }
            else if (viewportY + metrics.Height > edgeY)
            {
                viewportPoint.Y += viewportY - edgeY + metrics.Height;
            }
            else if (viewportY < edgeY && viewportPoint.Y < 0)
            {
                viewportPoint.Y = MathHelper.Min(viewportPoint.Y - viewportY + edgeY, 0);
            }
            #endregion
            #endregion
            ViewportPoint = new Point(MathI.Round(viewportPoint.X), MathI.Round(viewportPoint.Y));
        }

        protected override bool RenderContent(DirtyAreaCollector collector)
        {
            D2D1DeviceContext context = Renderer.GetDeviceContext();
            D2D1Brush[] brushes = _brushes;
            Rect bounds = ContentBounds;
            bool focused = _focused;
            RenderBackground(context, Enabled ? brushes[(int)Brush.BackBrush] : brushes[(int)Brush.BackDisabledBrush]);

            GetTextLayouts(out DWriteTextLayout? layout, out DWriteTextLayout? watermarkLayout);
            collector.MarkAsDirty(bounds);
            if (layout is null || (layout.DetermineMinWidth() <= 0.0f && (!_multiLine || !SequenceHelper.Contains(_text, '\n'))))
            {
                if (watermarkLayout is null)
                    return true;
                SetRenderingProperties(watermarkLayout, bounds, _multiLine);
                //文字為空，繪製浮水印
                PointF layoutPoint = bounds.Location;
                context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
                RenderLayoutCore(context, brushes[(int)Brush.ForeInactiveBrush], watermarkLayout, layoutPoint);
                context.PopAxisAlignedClip();
                if (focused)
                    DrawCaret(context, watermarkLayout, layoutPoint, 0);
                if (layout is not null)
                    DisposeHelper.NullSwapOrDispose(ref _layout, layout);
                DisposeHelper.NullSwapOrDispose(ref _watermarkLayout, watermarkLayout);
                return true;
            }

            SetRenderingProperties(layout, bounds, _multiLine);
            context.PushAxisAlignedClip((RectF)bounds, D2D1AntialiasMode.Aliased);
            RenderLayout(context, focused, layout, bounds);
            context.PopAxisAlignedClip();
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
            if (watermarkLayout is not null)
                DisposeHelper.NullSwapOrDispose(ref _watermarkLayout, watermarkLayout);

            return true;
        }

        private void RenderLayout(D2D1DeviceContext context, bool focused, DWriteTextLayout layout, in Rect layoutRect)
        {
            D2D1Brush[] brushes = _brushes;
            PointF viewportPoint = ViewportPoint;
            //輸出處理 (IME 作用中範圍提示、選取範圍提示、取得視角點等等)
            DWriteTextRange compositionRange = this.compositionRange;
            bool inComposition = compositionRange.Length > 0;
            //取得視角點
            PointF layoutPoint = new PointF(layoutRect.Left - viewportPoint.X, layoutRect.Top - viewportPoint.Y);
            //IME 作用中範圍提示
            if (inComposition)
                layout.SetUnderline(true, compositionRange);
            //選取範圍提示
            SelectionRange selectionRange = this.selectionRange;
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
                    for (int i = 0; i < length; i++)
                    {
                        DWriteHitTestMetrics rangeMetrics = metricsArray![i];
                        RectF selectionBounds = GraphicsUtils.AdjustRectangleF(new RectangleF(layoutPoint.X + rangeMetrics.Left, layoutPoint.Y + rangeMetrics.Top, rangeMetrics.Width, rangeMetrics.Height));
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

        private static void RenderLayoutCore(D2D1DeviceContext context, D2D1Brush foreBrush, DWriteTextLayout layout, in PointF point)
        {
            //繪製文字
            context.DrawTextLayout(point, layout, foreBrush, D2D1DrawTextOptions.None);
        }

        private void DrawCaret(D2D1DeviceContext context, DWriteTextLayout layout, in PointF layoutPoint, int caretIndex)
        {
            if (!_caretState)
                return;
            DWriteHitTestMetrics rangeMetrics = layout.HitTestTextRange(MathHelper.MakeUnsigned(caretIndex), 0, 0, 0)[0];
            float visualCaretX1 = MathF.Floor(layoutPoint.X + rangeMetrics.Left) + 0.5f;
            float visualCaretY1 = layoutPoint.Y + rangeMetrics.Top;
            float visualCaretY2 = visualCaretY1 + rangeMetrics.Height;
            PointF startPoint = new PointF(visualCaretX1, MathF.Floor(visualCaretY1));
            PointF endPoint = new PointF(visualCaretX1, MathF.Floor(visualCaretY2));
            context.DrawLine(startPoint, endPoint, _brushes[(int)Brush.ForeBrush], Renderer.GetBaseLineWidth());
        }

        #region Normal Key Controls

        public void OnKeyDown(System.Windows.Forms.KeyEventArgs args)
        {
            if (!_focused || !Enabled || args.Handled)
                return;
            KeyDown?.Invoke(this, args);
            if (args.Handled || args.Alt)
                return;
            bool isCtrlPressed = args.Control;
            bool isShiftPressed = args.Shift;
            bool justCtrlPressed = isCtrlPressed && !isShiftPressed;
            Keys keyCode = args.KeyCode;
            switch (keyCode)
            {
                case Keys.X when justCtrlPressed: // Ctrl + X
                    Cut();
                    break;
                case Keys.C when justCtrlPressed: // Ctrl + C
                    Copy();
                    break;
                case Keys.V when justCtrlPressed: // Ctrl + V
                    Paste();
                    break;
                case Keys.A when justCtrlPressed: // Ctrl + A
                    SelectAll();
                    break;
                case Keys.Delete:
                    DeleteOne();
                    break;
                case Keys.Left when isCtrlPressed:
                case Keys.Home:
                    MoveToStart(isShiftPressed);
                    break;
                case Keys.Right when isCtrlPressed:
                case Keys.End:
                    MoveToEnd(isShiftPressed);
                    break;
                case Keys.Left:
                    MoveLeft(isShiftPressed);
                    break;
                case Keys.Right:
                    MoveRight(isShiftPressed);
                    break;
                case Keys.Up:
                    MoveUp();
                    break;
                case Keys.Down:
                    MoveDown();
                    break;
                case Keys.Enter:
                    NextLine();
                    break;
            }
        }

        public void OnKeyUp(System.Windows.Forms.KeyEventArgs args)
        {
            if (_focused && Enabled)
            {
                KeyUp?.Invoke(this, args);
            }
        }
        #endregion

        #region IME Support
        void IIMEControl.StartIMEComposition()
        { }

        void IIMEControl.OnIMEComposition(string str, IMECompositionFlags flags, int cursorPosition)
        {
            RemoveSelection();
            if (cursorPosition < 0)
                cursorPosition = str.Length;
            DWriteTextRange range = compositionRange;
            if (range.Length <= 0)
            {
                range.StartPosition = MathHelper.MakeUnsigned(CaretIndex);
                range.Length = MathHelper.MakeUnsigned(str.Length);
                _compositionCaretIndex = cursorPosition;
                compositionRange = range;
                Text = Text.Insert(MathHelper.MakeSigned(range.StartPosition), str);
                return;
            }
            StringBuilderTiny builder = new StringBuilderTiny();
            if (Limits.UseStackallocStringBuilder)
            {
                unsafe
                {
                    char* buffer = stackalloc char[Limits.MaxStackallocChars];
                    builder.SetStartPointer(buffer, Limits.MaxStackallocChars);
                }
            }
            builder.Append(Text);
            builder.Remove(MathHelper.MakeSigned(compositionRange.StartPosition), MathHelper.MakeSigned(range.Length));
            builder.Insert(MathHelper.MakeSigned(range.StartPosition), str);
            range.Length = MathHelper.MakeUnsigned(str.Length);
            _compositionCaretIndex = cursorPosition;
            compositionRange = range;
            Text = builder.ToString();
            builder.Dispose();
        }

        void IIMEControl.OnIMECompositionResult(string str, IMECompositionFlags flags)
        {
            if (compositionRange.Length > 0)
            {
                StringBuilderTiny builder = new StringBuilderTiny();
                if (Limits.UseStackallocStringBuilder)
                {
                    unsafe
                    {
                        char* buffer = stackalloc char[Limits.MaxStackallocChars];
                        builder.SetStartPointer(buffer, Limits.MaxStackallocChars);
                    }
                }
                builder.Append(Text);
                builder.Remove(MathHelper.MakeSigned(compositionRange.StartPosition), MathHelper.MakeSigned(compositionRange.Length));
                builder.Insert(MathHelper.MakeSigned(compositionRange.StartPosition), str);
                Text = builder.ToString();
                builder.Dispose();
                CaretIndex += MathHelper.MakeSigned(compositionRange.Length);
                _compositionCaretIndex = 0;
                compositionRange.Length = 0;
                Update();
            }
            else
            {
                int length = str.Length;
                RemoveSelection();
                string newText = Text.Insert(CaretIndex, str);
                Text = newText;
                CaretIndex += length;
            }
        }

        void IIMEControl.EndIMEComposition()
        {
            if (compositionRange.Length > 0)
            {
                CaretIndex += MathHelper.MakeSigned(compositionRange.Length);
                compositionRange.Length = 0;
                Update();
            }
        }

        void ICharacterEvents.OnCharacterInput(char character)
        {
            if (!_focused || !Enabled)
                return;
            if (character < '\u0020')
            {
                switch (character)
                {
                    case '\b': // Backspace
                        if (selectionRange.Length > 0)
                        {
                            RemoveSelection();
                        }
                        else if (_caretIndex > 0)
                        {
                            CaretIndex--;
                            Text = Text.Remove(_caretIndex, 1);
                        }
                        break;
                }
                return;
            }
            switch (character)
            {
                case '\u007f': //DEL (Ctrl + Backspace)
                    if (selectionRange.Length > 0)
                    {
                        RemoveSelection();
                    }
                    else
                    {
                        int caretIndex = CaretIndex;
                        if (caretIndex > 0)
                        {
                            string text = Text;
                            int lastIndex = text.Length - 1;
                            int index;
                            int startIndex = MathHelper.Min(CaretIndex, lastIndex);
                            caretIndex = startIndex + 1;
                            do
                            {
                                index = text.LastIndexOf(' ', startIndex);
                                if (index >= startIndex)
                                    startIndex = index - 1;
                                else
                                    break;
                            } while (index > 0);
                            if (index < 0) index = 0;
                            else index++;
                            Text = text.Remove(index, caretIndex - index);
                            CaretIndex = index;
                        }
                    }
                    break;
                default:
                    RemoveSelection();
                    string newText = Text.Insert(_caretIndex, character.ToString());
                    Text = newText;
                    CaretIndex++;
                    break;
            }
        }
        #endregion

        #region TextBox Functions
        public void Cut()
        {
            string text = selectionRange.Length <= 0 ? string.Empty : _text.Substring(MathHelper.MakeSigned(selectionRange.ToTextRange().StartPosition), selectionRange.Length);
            RemoveSelection();
            if (_window.InvokeRequired)
            {
                _window.Invoke(new Action(() => System.Windows.Forms.Clipboard.SetText(text)));
            }
            else
            {
                System.Windows.Forms.Clipboard.SetText(text);
            }
        }

        public void Copy()
        {
            string text = selectionRange.Length <= 0 ? string.Empty : _text.Substring(MathHelper.MakeSigned(selectionRange.ToTextRange().StartPosition), selectionRange.Length);
            if (_window.InvokeRequired)
            {
                _window.Invoke(new Action(() => System.Windows.Forms.Clipboard.SetText(text)));
            }
            else
            {
                System.Windows.Forms.Clipboard.SetText(text);
            }
        }

        public void Paste()
        {
            string? text = null;
            if (_window.InvokeRequired)
                _window.Invoke(new Action(() => text = System.Windows.Forms.Clipboard.GetText()));
            else
                text = System.Windows.Forms.Clipboard.GetText();
            RemoveSelection();
            if (StringHelper.IsNullOrEmpty(text))
                return;
            Text = _text.Insert(CaretIndex, text);
            CaretIndex += text.Length;
        }

        public void SelectAll()
        {
            int length = Text.Length;
            SelectionRange selectionRange = this.selectionRange;
            selectionRange.StartPosition = 0;
            selectionRange.Length = length;
            this.selectionRange = selectionRange;
            CaretIndex = length;
            Update();
        }

        public void RemoveSelection()
        {
            SelectionRange selectionRange = this.selectionRange;
            int selectionLength = selectionRange.Length;
            if (selectionLength <= 0)
                return;
            DWriteTextRange range = selectionRange.ToTextRange();
            CaretIndex = MathHelper.MakeSigned(compositionRange.StartPosition);
            Text = Text.Remove(MathHelper.MakeSigned(range.StartPosition), selectionLength);
            selectionRange.Length = 0;
            this.selectionRange = selectionRange;
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private void DeleteOne()
        {
            string text = Text;
            if (StringHelper.IsNullOrEmpty(text))
                return;
            if (selectionRange.Length > 0)
            {
                RemoveSelection();
                return;
            }
            int caretIndex = _caretIndex;
            int length = text.Length;
            if (caretIndex >= length)
            {
                if (caretIndex > 0)
                {
                    CaretIndex = --caretIndex;
                    Text = text.Remove(caretIndex, 1);
                }
                return;
            }
            if (caretIndex >= 0)
            {
                Text = text.Remove(caretIndex, 1);
            }
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveToStart(bool isSelectionMode)
        {
            bool isInComposition = compositionRange.Length > 0;
            int selectionStartPos = 0, selectionEndPos = 0;
            if (isSelectionMode)
            {
                if (selectionRange.Length == 0)
                {
                    if (isInComposition)
                    {
                        selectionEndPos = CaretIndex + _compositionCaretIndex;
                    }
                    else
                    {
                        selectionEndPos = CaretIndex;
                    }
                }
                else
                {
                    selectionEndPos = MathHelper.Max(selectionRange.StartPosition, selectionRange.EndPosition);
                }
            }
            if (isInComposition)
            {
                _compositionCaretIndex = 0;
                if (isSelectionMode)
                {
                    selectionStartPos = CaretIndex;
                }
            }
            else
            {
                CaretIndex = 0;
            }
            selectionRange.StartPosition = selectionStartPos;
            selectionRange.EndPosition = selectionEndPos;
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveToEnd(bool isSelectionMode)
        {
            int caretIndex = CaretIndex;
            int compositionLength = MathHelper.MakeSigned(compositionRange.Length);
            int textLength = Text.Length, selectionStartPos = textLength, selectionEndPos = textLength;
            bool isInComposition = compositionLength > 0;
            SelectionRange selectionRange = this.selectionRange;
            if (isSelectionMode)
            {
                if (selectionRange.Length == 0)
                {
                    if (isInComposition)
                        selectionStartPos = caretIndex + _compositionCaretIndex;
                    else
                        selectionStartPos = caretIndex;
                }
                else
                {
                    selectionStartPos = MathHelper.Min(selectionRange.StartPosition, selectionRange.EndPosition);
                }
            }
            if (isInComposition)
            {
                _compositionCaretIndex = compositionLength;
                if (isSelectionMode)
                    selectionEndPos = caretIndex + compositionLength;
            }
            else
            {
                CaretIndex = textLength;
            }
            selectionRange.StartPosition = selectionStartPos;
            selectionRange.EndPosition = selectionEndPos;
            this.selectionRange = selectionRange;
            Update();
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveLeft(bool isSelectionMode)
        {
            if (compositionRange.Length > 0)
            {
                _compositionCaretIndex = MathHelper.Max(_compositionCaretIndex - 1, 0);
                selectionRange.Length = 0;
                Update();
                return;
            }
            int caretIndex = CaretIndex;
            bool selection = false;
            if (isSelectionMode)
            {
                selection = true;
                if (selectionRange.Length <= 0)
                {
                    selectionRange.StartPosition = caretIndex;
                }
            }
            else
            {
                selectionRange.Length = 0;
            }
            caretIndex--;
            if (selection)
                selectionRange.EndPosition = caretIndex;
            CaretIndex = caretIndex;
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveRight(bool isSelectionMode)
        {
            int compositionLength = MathHelper.MakeSigned(compositionRange.Length);
            if (compositionLength > 0)
            {
                _compositionCaretIndex = MathHelper.Min(_compositionCaretIndex + 1, compositionLength);
                selectionRange.Length = 0;
                Update();
                return;
            }
            int caretIndex = CaretIndex;
            bool selection = false;
            if (isSelectionMode)
            {
                selection = true;
                if (selectionRange.Length <= 0)
                {
                    selectionRange.StartPosition = caretIndex;
                }
            }
            else
            {
                selectionRange.Length = 0;
            }
            caretIndex++;
            if (selection)
            {
                selectionRange.EndPosition = caretIndex;
            }
            CaretIndex = caretIndex;
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveUp()
        {
            if (!MultiLine)
                return;
            int caretIndex = CaretIndex;
            if (caretIndex <= 0)
                return;

            using DWriteTextLayout layout = CreateVirtualTextLayout();
            layout.HitTestTextPosition(MathHelper.MakeUnsigned(caretIndex), false, out float pointX, out float pointY);

            pointY -= 5;
            if (pointY < 0)
                return;

            int pos = MathHelper.MakeSigned(layout.HitTestPoint(pointX, pointY, out bool isTrailingHit, out bool isInside).TextPosition);
            if (isTrailingHit)
            {
                int textLength = Text.Length;
                if (pos < textLength - 1)
                {
                    if (isInside)
                        pos++;
                }
                else
                    pos = textLength;
            }
            CaretIndex = pos;
        }

        [Inline(InlineBehavior.Remove)]
        private void MoveDown()
        {
            if (!MultiLine)
                return;
            int caretIndex = CaretIndex;
            int textLength = Text.Length;
            if (caretIndex >= textLength)
                return;
            using DWriteTextLayout layout = CreateVirtualTextLayout();
            DWriteHitTestMetrics metrics = layout.HitTestTextPosition(MathHelper.MakeUnsigned(caretIndex), false, out float pointX, out float pointY);
            pointY += metrics.Height + 5;
            if (pointY < 0)
                return;
            int pos = MathHelper.MakeSigned(layout.HitTestPoint(pointX, pointY, out bool isTrailingHit, out bool isInside).TextPosition);
            if (isTrailingHit)
            {
                if (pos < textLength - 1)
                {
                    if (isInside)
                        pos++;
                }
                else
                    pos = textLength;
            }
            CaretIndex = pos;
        }

        [Inline(InlineBehavior.Remove)]
        private void NextLine()
        {
            if (!MultiLine)
                return;
            int caretIndex = CaretIndex;
            Text = Text.Insert(caretIndex, "\n");
            CaretIndex = caretIndex + 1;
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
            using DWriteTextLayout layout = CreateVirtualTextLayout();
            int result = MathHelper.MakeSigned(layout.HitTestPoint(point.X - viewportLeft, point.Y - viewportTop, out bool isTrailingHit, out isInside).TextPosition);
            if (isTrailingHit)
            {
                int textLength = text.Length;
                if (result < textLength - 1)
                {
                    if (isInside)
                        result++;
                }
                else
                    result = textLength;
            }
            return result;
        }

        #region Mouse Events Handling

        int clicks = 0;
        long lastClickedTime = long.MinValue;
        bool drag = false;
        SelectionRange previousSelectionRange;
        public override void OnMouseDown(in MouseInteractEventArgs args)
        {
            base.OnMouseDown(args);
            if (args.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
                return;
            bool contained = ContentBounds.Contains(args.Location);
            if (contained && Enabled && args.Button.HasFlag(System.Windows.Forms.MouseButtons.Left))
            {
                drag = true;
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
                        previousSelectionRange = selectionRange = new SelectionRange(caretIndex, caretIndex);
                        break;
                    default:
                        string text = Text;
                        int textLength = text.Length;
                        if (textLength > 0)
                        {
                            switch ((clicks - 1) % 2)
                            {
                                case 0:
                                    previousSelectionRange = selectionRange = new SelectionRange(0, textLength);
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
                                            previousSelectionRange = selectionRange = new SelectionRange(selectionStart, selectionEnd);
                                            caretIndex = selectionEnd;
                                        }
                                        else
                                        {
                                            int selectionStart = text.LastIndexOf(' ', caretIndex) + 1;
                                            int selectionEnd = StringHelper.IndexOf(text, ' ', caretIndex);
                                            if (selectionStart < 0) selectionStart = 0;
                                            if (selectionEnd < 0) selectionEnd = text.Length;
                                            previousSelectionRange = selectionRange = new SelectionRange(selectionStart, selectionEnd);
                                            caretIndex = selectionEnd;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
                if (CaretIndex != caretIndex || !isInside)
                {
                    CaretIndex = caretIndex;
                }
            }
            else
            {
                if (!contained)
                {
                    drag = false;
                }
                if (selectionRange.Length > 0)
                {
                    selectionRange.Length = 0;
                    Update();
                }
            }
            bool focused = _focused;
            if (contained != focused)
            {
                if (contained)
                    _window.ChangeFocusElement(this);
                else
                    _window.ClearFocusElement(this);
            }
        }

        bool isEnter = false;
        public override void OnMouseMove(in MouseInteractEventArgs args)
        {
            base.OnMouseMove(args);
            if (ContentBounds.Contains(args.Location))
            {
                if (!isEnter)
                {
                    _cursor = System.Windows.Forms.Cursors.IBeam;
                    isEnter = true;
                }
            }
            else
            {
                if (isEnter)
                {
                    _cursor = null;
                    isEnter = false;
                }
            }
            if (drag)
            {
                string text = _text;
                if (StringHelper.IsNullOrEmpty(text))
                    return;
                PointF location = args.Location;
                if (!_multiLine)
                {
                    using DWriteTextLayout layout = CreateVirtualTextLayout();
                    location.Y = Location.Y + 3 + layout.GetMetrics().Top;
                }
                int newCaretIndex = GetCaretIndexFromPoint(location, out _);
                if (CaretIndex != newCaretIndex)
                {
                    int previousSelectionStart = previousSelectionRange.StartPosition;
                    int previousSelectionEnd = previousSelectionRange.EndPosition;
                    if (newCaretIndex < previousSelectionStart)
                        selectionRange.StartPosition = newCaretIndex;
                    else if (newCaretIndex > previousSelectionEnd)
                        selectionRange.EndPosition = newCaretIndex;
                    CaretIndex = newCaretIndex;
                    Update();
                }
            }
        }

        public override void OnMouseUp(in MouseInteractEventArgs args)
        {
            base.OnMouseUp(args);
            drag = false;
            if (Enabled && RequestContextMenu != null && args.Button == System.Windows.Forms.MouseButtons.Right && ContentBounds.Contains(args.Location))
            {
                RequestContextMenu.Invoke(this, args);
            }
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
    }
}
