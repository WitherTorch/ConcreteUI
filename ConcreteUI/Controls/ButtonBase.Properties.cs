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
                ButtonTriState result;
                OptimisticLock.Enter(ref _version, out uint version);
                do
                {
                    result = _pressState;
                } while (OptimisticLock.TryLeave(ref _version, ref version));
                return result;
            }
        }

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                bool result;
                OptimisticLock.Enter(ref _version, out uint version);
                do
                {
                    result = _enabled;
                } while (OptimisticLock.TryLeave(ref _version, ref version));
                return result;
            }
            set
            {
                if (ReferenceHelper.Exchange(ref _enabled, value) == value)
                    return;
                _version++;
                Update();
            }
        }
    }
}
