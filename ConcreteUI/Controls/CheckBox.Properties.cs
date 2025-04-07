using System.Runtime.CompilerServices;
using System;

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
                Update(CheckBoxDrawType.RedrawCheckBox);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _text = value;
                _generateLayout = true;
                Update();
            }
        }
    }
}
