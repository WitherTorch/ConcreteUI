using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class ButtonBase
    {
        public event MouseNotifyEventHandler? Click;

        protected ButtonTriState PressState
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
