using System.Runtime.CompilerServices;
using System;
using System.Threading;
using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        public bool Checked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _checkState;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_checkState == value)
                    return;
                _checkState = value;
                CheckedChanged?.Invoke(this, EventArgs.Empty);
                Update(RedrawType.RedrawCheckBox);
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
                if (ReferenceEquals(_text, value))
                    return;
                _text = value;
                Update(RenderObjectUpdateFlags.Layout);
            }
        }
    }
}
