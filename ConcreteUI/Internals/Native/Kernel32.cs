using System;

namespace ConcreteUI.Internals.Native
{
    internal static unsafe partial class Kernel32
    {
        public static partial uint GetCurrentThreadId();
        public static partial IntPtr GetModuleHandleW(char* lpModuleName);
        public static partial IntPtr GlobalAlloc(GlobalAllocFlags uFlags, nuint dwBytes);
        public static partial IntPtr GlobalFree(IntPtr hMem);
        public static partial void* GlobalLock(IntPtr hMem);
        public static partial bool GlobalUnlock(IntPtr hMem);
        public static partial IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);
        public static partial bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);
        public static partial bool CloseHandle(IntPtr hObject);
        public static partial int GetLastError();
    }
}
