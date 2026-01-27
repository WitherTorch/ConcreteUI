using System;
using System.Threading;

using ConcreteUI.Graphics.Internals.Native;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private sealed class RenderingThreadLegacy : RenderingThreadBase
        {
            public RenderingThreadLegacy(RenderingController controller, uint framesPerSecond) : base(controller, framesPerSecond) { }

            protected override unsafe IntPtr CreateWaitingHandle(bool manualReset)
                => Kernel32.CreateEventW(null, bManualReset: manualReset, bInitialState: false, null);

            protected override void DestroyWaitingHandle(IntPtr waitingHandle)
                => Kernel32.CloseHandle(waitingHandle);

            protected override void WakeUp(IntPtr waitingHandle)
                => Kernel32.SetEvent(waitingHandle);

            protected override bool Wait(IntPtr waitingHandle, uint timeout)
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
