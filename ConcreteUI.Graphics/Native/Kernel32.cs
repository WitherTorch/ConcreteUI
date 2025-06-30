using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Kernel32
    {
        private const string KERNEL32_DLL = "kernel32.dll";

        [DllImport(KERNEL32_DLL)]
        public static extern bool QueryPerformanceFrequency(long* lpFrequency);

        [DllImport(KERNEL32_DLL)]
        public static extern bool QueryPerformanceCounter(long* lpPerformanceCount);

        [DllImport(KERNEL32_DLL)]
        public static extern void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);

        [DllImport(KERNEL32_DLL)]
        public static extern IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);

        [DllImport(KERNEL32_DLL)]
        public static extern bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);

        [DllImport(KERNEL32_DLL)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport(KERNEL32_DLL)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
