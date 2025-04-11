using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        #region Events
        public event MouseInteractEventHandler? RequestContextMenu;
        public event KeyEventHandler? KeyDown;
        public event KeyEventHandler? KeyUp;
        public event TextChangingEventHandler? TextChanging;
        public event EventHandler? TextChanged;
        #endregion

        #region Properties
        public Cursor? PredicatedCursor => _cursor;

        public TextAlignment Alignment
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _alignment;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_alignment == value)
                    return;
                _alignment = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.SwapDisposeInterlocked(ref _watermarkLayout);
                Update(RenderObjectUpdateFlags.Format);
            }
        }

        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fontSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_fontSize == value)
                    return;
                _fontSize = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                DisposeHelper.SwapDisposeInterlocked(ref _watermarkLayout);
                Update(RenderObjectUpdateFlags.Format);
            }
        }

        public int CaretIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _caretIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int oldCaretIndex = _caretIndex;
                if (oldCaretIndex == value)
                    return;
                value = MathHelper.Max(MathHelper.Min(value, _text.Length), 0);
                if (oldCaretIndex == value)
                    return;
                _caretIndex = value;
                _caretState = true;
                if (Enabled)
                    _caretTimer.Change(500, 500);
                CalculateCurrentViewportPoint();
                Update();
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                value = FixString(value);

                if (Renderer.IsInitializingElements())
                {
                    _text = value;
                    return;
                }

                TextChangingEventHandler? changingHandler = TextChanging;
                if (changingHandler is not null)
                {
                    TextChangingEventArgs args = new TextChangingEventArgs(value);
                    changingHandler.Invoke(this, args);
                    if (args is not null)
                    {
                        if (args.IsCanceled)
                            return;
                        if (args.IsEdited)
                            value = FixString(args.Text);
                    }
                }

                _text = value;

                int length = value.Length;

                selectionRange.Length = 0;
                _caretIndex = MathHelper.Clamp(_caretIndex, 0, length);

                TextChanged?.Invoke(this, EventArgs.Empty);

                if (_multiLine)
                {
                    using DWriteTextLayout layout = CreateVirtualTextLayout();
                    layout.MaxWidth = ContentBounds.Width;
                    SurfaceSize = new Size(0, MathI.Ceiling(layout.GetMetrics().Height) + UIConstants.ElementMargin);
                }
                CalculateCurrentViewportPoint();
                Update(RenderObjectUpdateFlags.Layout);
            }
        }

        public string Watermark
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _watermark;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                value = FixString(value);
                if (_watermark == value)
                    return;
                _watermark = value;

                Update(RenderObjectUpdateFlags.WatermarkLayout);
            }
        }

        public bool MultiLine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _multiLine;
            set
            {
                if (_multiLine == value)
                    return;
                _multiLine = value;

                if (value)
                {
                    Rect bounds = ContentBounds;
                    if (bounds.IsEmpty)
                        SurfaceSize = Size.Empty;
                    else
                    {
                        using DWriteTextLayout layout = CreateVirtualTextLayout();
                        layout.MaxWidth = bounds.Width;
                        SurfaceSize = new Size(0, MathI.Ceiling(layout.GetMetrics().Height) + UIConstants.ElementMargin);
                    }
                }
                else
                {
                    Text = FixString(_text);
                    SurfaceSize = new Size(int.MaxValue, 0);
                }
                Update();
            }
        }

        public bool IMEEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _imeEnabled;
            set
            {
                if (_imeEnabled == value)
                    return;
                _imeEnabled = value;

                if (value && Enabled && _focused)
                    _ime.Attach(this);
                else
                    _ime.Detach(this);
            }
        }

        public char PasswordChar
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _passwordChar;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_passwordChar == value)
                    return;
                _passwordChar = value;
                Update(RenderObjectUpdateFlags.Layout);
            }
        }

        public bool HasSelection => selectionRange.Length > 0;
        #endregion
    }
}
