using System;
using System.Threading;

using ConcreteUI.Graphics.Internals.Native;

namespace ConcreteUI.Graphics.Internals
{
    partial class WaitingEventManager
    {
        private sealed class LegacyImpl : IWaitingEventManager
        {
            public unsafe IntPtr CreateWaitingHandle(bool manualReset)
                => Kernel32.CreateEventW(null, bManualReset: manualReset, bInitialState: false, null);

            public void DestroyWaitingHandle(IntPtr waitingHandle)
                => Kernel32.CloseHandle(waitingHandle);

            public void WakeUp(IntPtr waitingHandle)
                => Kernel32.SetEvent(waitingHandle);

            public bool Wait(IntPtr waitingHandle, uint timeout)
            {
                const uint INFINITE = unchecked((uint)Timeout.Infinite);
                const uint WAIT_TIMEOUT = 0x00000102U;

                if (timeout == INFINITE)
                {
                    Kernel32.WaitForSingleObject(waitingHandle, dwMilliseconds: timeout);
                    return true;
                }

                return Kernel32.WaitForSingleObject(waitingHandle, dwMilliseconds: timeout) != WAIT_TIMEOUT;
            }
        }
    }
}
