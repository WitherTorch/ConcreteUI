#if NET8_0_OR_GREATER
using System;

namespace ConcreteUI.Internals
{
    partial class WindowClassImpl
    {
        private static unsafe partial delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint> GetWndProcPointer()
            => (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)&ProcessWindowMessage;
    }
}
#endif