using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    partial class ButtonBase
    {
        public event MouseNotifyEventHandler? Click;

        protected ButtonTriState PressState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly uint valueRef = ref _pressState;
                ref readonly nuint versionRef = ref _version;

                uint value = OptimisticLock.EnterWithPrimitive(in valueRef, in versionRef, out nuint version);
                while (!OptimisticLock.TryLeaveWithPrimitive(in valueRef, in versionRef, ref value, ref version)) ;
                return (ButtonTriState)value;
            }
        }

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly bool valueRef = ref _enabled;
                ref readonly nuint versionRef = ref _version;

                bool value = OptimisticLock.EnterWithPrimitive(in valueRef, in versionRef, out nuint version);
                while (!OptimisticLock.TryLeaveWithPrimitive(in valueRef, in versionRef, ref value, ref version)) ;
                return value;
            }
            set
            {
                if (ReferenceHelper.Exchange(ref _enabled, value) == value)
                    return;
                OptimisticLock.Increase(ref _version);
                Update();
            }
        }
    }
}
