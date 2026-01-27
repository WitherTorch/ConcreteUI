using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Internals.Native;

namespace ConcreteUI.Graphics
{
    partial class RenderingController
    {
        private static class RenderingThreadHelper
        {
            private static readonly bool _isSupportedModernRenderingThread;

            static RenderingThreadHelper()
            {
                _isSupportedModernRenderingThread =
                    KernelBase.CheckSupported(nameof(KernelBase.WaitOnAddress)) &&
                    KernelBase.CheckSupported(nameof(KernelBase.WakeByAddressAll));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static RenderingThreadBase CreateRenderingThread(RenderingController controller, uint framesPerSecond)
                => _isSupportedModernRenderingThread ?
                new RenderingThreadModern(controller, framesPerSecond) :
                new RenderingThreadLegacy(controller, framesPerSecond);
        }
    }
}
