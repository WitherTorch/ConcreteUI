using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class TextButton
    {
        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            set
            {
                if (ReferenceEquals(_text, value))
                    return;
                _text = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                Update();
            }
        }

        public string FontName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fontName;
            set
            {
                if (ReferenceEquals(_fontName, value))
                    return;
                _fontName = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                Update();
            }
        }

        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoFontSize ? 0 : _fontSize;
            set
            {
                if (!_autoFontSize && _fontSize == value)
                    return;
                _autoFontSize = false;
                _fontSize = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                Update();
            }
        }

        public float AutoFontSizeScale
        {
            get => _autoFontSizeScale;
            set
            {
                if (_autoFontSizeScale == value)
                    return;
                _autoFontSizeScale = value;
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                Update();
            }
        }
    }
}
