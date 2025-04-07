using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Controls
{
    partial class Label
    {
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
                Update(RenderObjectUpdateFlags.FormatAndLayout);
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
                Update(RenderObjectUpdateFlags.FormatAndLayout);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_text == value)
                    return;
                _text = value;
                Update(RenderObjectUpdateFlags.Layout);
            }
        }

        public bool WordWrap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _wordWrap;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_wordWrap == value)
                    return;
                _wordWrap = value;
                Update();
            }
        }
    }
}
