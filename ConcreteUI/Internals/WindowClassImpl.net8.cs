#if NET8_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Internals
{
    partial class WindowClassImpl
    {
        private static unsafe partial delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint> GetWndProcPointer()
            => (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)&ProcessWindowMessage;

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static unsafe partial nint ProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam);
    }
}
#endif