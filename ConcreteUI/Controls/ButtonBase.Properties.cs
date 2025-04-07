using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class ButtonBase
    {
        protected ButtonPressState PressState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pressState;
        }

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                Update();
            }
        }
    }
}
