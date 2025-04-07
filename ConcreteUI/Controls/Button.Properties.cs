using System;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class Button
    {
        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fontSize;
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
            set
            {
                if (_text == value)
                    return;
                _text = value;
                Update(RenderObjectUpdateFlags.Layout);
            }
        }
    }
}
