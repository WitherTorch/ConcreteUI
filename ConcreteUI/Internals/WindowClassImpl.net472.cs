#if NET472_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace ConcreteUI.Internals
{
    partial class WindowClassImpl
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate nint WndProcDelegate(IntPtr hwnd, uint msg, nint wParam, nint lParam);

        private static readonly WndProcDelegate _delegate = ProcessWindowMessage;

        private static unsafe partial delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint> GetWndProcPointer()
            => (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)Marshal.GetFunctionPointerForDelegate(_delegate);
    }
}
#endif