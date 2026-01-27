using System;

namespace ConcreteUI.Graphics.Native
{
    internal static unsafe partial class Kernel32
    {
        public static partial bool QueryPerformanceFrequency(long* lpFrequency);
        public static partial bool QueryPerformanceCounter(long* lpPerformanceCount);
        public static partial void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);
        public static partial IntPtr CreateEventW(void* lpEventAttributes, bool bManualReset, bool bInitialState, char* lpName);
        public static partial IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);
        public static partial bool SetEvent(IntPtr hEvent);
        public static partial bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);
        public static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        public static partial uint WaitForMultipleObjects(uint nCount, IntPtr* lpHandles, bool bWaitAll, uint dwMilliseconds);
        public static partial bool CloseHandle(IntPtr hObject);
        public static partial uint GetLastError();
    }
}
