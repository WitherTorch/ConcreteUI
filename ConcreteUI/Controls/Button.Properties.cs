using System.Runtime.CompilerServices;

using ConcreteUI.Layout;

using WitherTorch.Common.Helpers;

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
                DisposeHelper.SwapDisposeInterlocked(ref _layout);
                Update(RenderObjectUpdateFlags.Format);
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
