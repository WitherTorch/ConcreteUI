#if NET472_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Kernel32
    {
        private const string KERNEL32_DLL = "kernel32.dll";

        [DllImport(KERNEL32_DLL)]
        public static extern partial uint GetCurrentThreadId();

        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GetModuleHandleW(char* lpModuleName);

        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GlobalAlloc(GlobalAllocFlags uFlags, nuint dwBytes);

        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GlobalFree(IntPtr hMem);

        [DllImport(KERNEL32_DLL)]
        public static extern partial void* GlobalLock(IntPtr hMem);

        [DllImport(KERNEL32_DLL)]
        public static extern partial bool GlobalUnlock(IntPtr hMem);

        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);

        [DllImport(KERNEL32_DLL)]
        public static extern partial bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);

        [DllImport(KERNEL32_DLL)]
        public static extern partial bool CloseHandle(IntPtr hObject);

        [DllImport(KERNEL32_DLL)]
        public static extern partial int GetLastError();
    }
}
#endif