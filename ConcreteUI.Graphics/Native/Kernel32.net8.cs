#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Kernel32
    {
        private const string KERNEL32_DLL = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool QueryPerformanceFrequency(long* lpFrequency);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool QueryPerformanceCounter(long* lpPerformanceCount);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr CreateEventW(void* lpEventAttributes, bool bManualReset, bool bInitialState, char* lpName);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool SetEvent(IntPtr hEvent);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);

        [DllImport(KERNEL32_DLL)]
        public static extern partial uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport(KERNEL32_DLL)]
        public static extern partial uint WaitForMultipleObjects(uint nCount, IntPtr* lpHandles, bool bWaitAll, int dwMilliseconds);

        [SuppressGCTransition]
        [DllImport(KERNEL32_DLL)]
        public static extern partial bool CloseHandle(IntPtr hObject);
    }
}
#endif