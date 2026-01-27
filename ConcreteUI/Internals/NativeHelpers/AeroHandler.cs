using System;

using ConcreteUI.Internals.Native;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class AeroHandler
    {
        public static unsafe void EnableBlur(IntPtr Handle)
        {
            DWMBlurBehind blurBehind = new DWMBlurBehind(true) { fTransitionOnMaximized = true };
            DwmApi.DwmEnableBlurBehindWindow(Handle, ref blurBehind);
        }

        public static unsafe bool HasBlur()
        {
            bool result;
            return DwmApi.DwmIsCompositionEnabled(&result) == 0 && result;
        }
    }
}
