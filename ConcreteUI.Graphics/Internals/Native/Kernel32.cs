using System;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Kernel32
    {
        private const string LibraryName = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool QueryPerformanceFrequency(long* lpFrequency);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool QueryPerformanceCounter(long* lpPerformanceCount);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern void GetSystemTimeAsFileTime(long* lpSystemTimeAsFileTime);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateEventW(void* lpEventAttributes, SysBool bManualReset, SysBool bInitialState, char* lpName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateWaitableTimerW(void* lpTimerAttributes, SysBool bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool SetEvent(IntPtr hEvent);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, SysBool fResume);

        [DllImport(LibraryName)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport(LibraryName)]
        public static extern uint WaitForMultipleObjects(uint nCount, IntPtr* lpHandles, SysBool bWaitAll, uint dwMilliseconds);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetLastError();
    }
}