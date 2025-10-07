#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Kernel32
    {
        private const string KERNEL32_DLL = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial uint GetCurrentThreadId();

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GetModuleHandleW(char* lpModuleName);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GlobalAlloc(GlobalAllocFlags uFlags, nuint dwBytes);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr GlobalFree(IntPtr hMem);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial void* GlobalLock(IntPtr hMem);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool GlobalUnlock(IntPtr hMem);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial int GetLastError();
    }
}
#endif