using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        #region Events
        public event MouseNotifyEventHandler? RequestContextMenu;
        public event KeyInteractEventHandler? KeyDown;
        public event KeyInteractEventHandler? KeyUp;
        public event TextChangingEventHandler? TextChanging;
        public event EventHandler? TextChanged;
        #endregion

        #region Properties
        public SystemCursorType? PredicatedCursor => _cursorType;

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
                value = AdjustCaretIndex(value, takeGreaterIfNotExists: false);
                if (oldCaretIndex == value)
                    return;
                UpdateCaretIndex(value);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UpdateTextAndCaretIndex(value, _caretIndex);
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

                string text = _text;
                if (value)
                {
                    Rect bounds = ContentBounds;
                    if (!bounds.IsValid)
                        SurfaceSize = Size.Empty;
                    else
                    {
                        using DWriteTextLayout layout = CreateVirtualTextLayout(text);
                        layout.MaxWidth = bounds.Width;

                        SurfaceSize = new Size(0, MathI.Ceiling(layout.GetMetrics().Height) + UIConstants.ElementMargin);
                    }
                }
                else
                {
                    SurfaceSize = new Size(int.MaxValue, 0);
                    Text = FixString(text);
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
                    _ime?.Attach(this);
                else
                    _ime?.Detach(this);
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

        public bool HasSelection => _selectionRange.Length > 0;

        public LayoutVariable AutoHeightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoLayoutVariableCache[0] ??= new AutoHeightVariable(this);
        }
        #endregion
    }
}
