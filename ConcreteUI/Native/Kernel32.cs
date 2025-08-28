using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class Kernel32
    {
        private const string KERNEL32_DLL = "kernel32.dll";

        [DllImport(KERNEL32_DLL)]
        public static extern int GetCurrentThreadId();

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr GetModuleHandleW(char* lpModuleName);

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr GlobalAlloc(GlobalAllocFlags uFlags, nuint dwBytes);

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport(KERNEL32_DLL)]
        public static extern void* GlobalLock(IntPtr hMem);

        [DllImport(KERNEL32_DLL)]
        public static extern bool GlobalUnlock(IntPtr hMem);
    }
}
