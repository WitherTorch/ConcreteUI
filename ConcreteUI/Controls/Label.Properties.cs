using System.Runtime.CompilerServices;

using ConcreteUI.Layout;

using WitherTorch.Common.Helpers;

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
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
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
                Update(RenderObjectUpdateFlags.Format);
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

        public LayoutVariable AutoWidthReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoLayoutVariableCache[0] ??= new AutoWidthVariable(this);
        }

        public LayoutVariable AutoHeightReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoLayoutVariableCache[1] ??= new AutoHeightVariable(this);
        }
    }
}
