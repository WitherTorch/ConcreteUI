using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Internals.Native;

namespace ConcreteUI.Graphics.Internals
{
    internal static partial class WaitingEventManager
    {
        private static readonly bool _isSupportedModernRenderingThread;

        static WaitingEventManager()
        {
            _isSupportedModernRenderingThread =
                KernelBase.CheckSupported(nameof(KernelBase.WaitOnAddress)) &&
                KernelBase.CheckSupported(nameof(KernelBase.WakeByAddressAll));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IWaitingEventManager Create()
            => _isSupportedModernRenderingThread ? new ModernImpl() : new LegacyImpl();
    }

    internal interface IWaitingEventManager
    {
        IntPtr CreateWaitingHandle(bool manualReset);

        void DestroyWaitingHandle(IntPtr waitingHandle);

        void WakeUp(IntPtr waitingHandle);

        bool Wait(IntPtr waitingHandle, uint timeout);
    }
}
