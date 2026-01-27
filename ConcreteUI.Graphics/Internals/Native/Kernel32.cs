using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Kernel32
    {
        private const string LibraryName = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool QueryPerformanceFrequency(long* lpFrequency);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool QueryPerformanceCounter(long* lpPerformanceCount);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateEventW(void* lpEventAttributes, bool bManualReset, bool bInitialState, char* lpName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateWaitableTimerW(void* lpTimerAttributes, bool bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool SetEvent(IntPtr hEvent);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, bool fResume);

        [DllImport(LibraryName)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport(LibraryName)]
        public static extern uint WaitForMultipleObjects(uint nCount, IntPtr* lpHandles, bool bWaitAll, uint dwMilliseconds);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetLastError();
    }
}