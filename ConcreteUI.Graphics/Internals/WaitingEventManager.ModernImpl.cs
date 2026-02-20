using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

using ConcreteUI.Graphics.Internals.Native;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Internals
{
    partial class WaitingEventManager
    {
        private sealed unsafe class ModernImpl : IWaitingEventManager
        {
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            private struct RawWaitingEvent
            {
                private readonly nuint _manualReset;
                private nuint _state;

                public readonly bool IsManuallyReset => _manualReset != 0;
                public bool State
                {
                    readonly get => _state != 0;
                    set => _state = value ? 1u : 0u;
                }

                public static IntPtr GetWaitingHandleFromEvent(RawWaitingEvent* source)
                    => (IntPtr)(&source->_state);

                public static RawWaitingEvent* GetEventFromWaitingHandle(IntPtr waitingHandle)
                    => (RawWaitingEvent*)(((nuint*)waitingHandle) - 1);

                public RawWaitingEvent(bool manualReset)
                {
                    _manualReset = manualReset ? 1u : 0u;
                    _state = 0;
                }
            }

            public IntPtr CreateWaitingHandle(bool manualReset)
            {
                RawWaitingEvent* ptr = NativeMethods.AllocUnmanagedStructure(new RawWaitingEvent(manualReset));
                return RawWaitingEvent.GetWaitingHandleFromEvent(ptr);
            }

            public void DestroyWaitingHandle(IntPtr waitingHandle)
            {
                RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(waitingHandle);
                NativeMethods.FreeMemory(ptr);
            }

            public void WakeUp(IntPtr waitingHandle)
            {
                RawWaitingEvent.GetEventFromWaitingHandle(waitingHandle)->State = true;
                KernelBase.WakeByAddressAll((void*)waitingHandle);
            }

            public bool Wait(IntPtr waitingHandle, uint timeout)
            {
                RawWaitingEvent* ptr = RawWaitingEvent.GetEventFromWaitingHandle(waitingHandle);
                if (ptr->IsManuallyReset)
                    return WaitCore(waitingHandle, timeout);
                try
                {
                    return WaitCore(waitingHandle, timeout);
                }
                finally
                {
                    ptr->State = false;
                }
            }

            private static bool WaitCore(IntPtr waitingHandle, uint timeout)
            {
                const uint INFINITE = unchecked((uint)Timeout.Infinite);
                const int ERROR_TIMEOUT = 0x5B4;

                nuint referenceValue = 0;

                bool result = KernelBase.WaitOnAddress((void*)waitingHandle, &referenceValue, UnsafeHelper.SizeOf<nuint>(), timeout);

                if (timeout == INFINITE)
                    return result;

                if (result)
                    return true;

                uint lastError = Kernel32.GetLastError();
                if (lastError != ERROR_TIMEOUT)
                    throw new Win32Exception((int)lastError);
                return false;
            }
        }
    }
}
