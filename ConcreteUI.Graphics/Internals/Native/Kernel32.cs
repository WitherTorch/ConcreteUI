using System;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Kernel32
    {
        private const string LibraryName = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool32 QueryPerformanceFrequency(long* lpFrequency);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool32 QueryPerformanceCounter(long* lpPerformanceCount);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateEventW(void* lpEventAttributes, SysBool32 bManualReset, SysBool32 bInitialState, char* lpName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateWaitableTimerW(void* lpTimerAttributes, SysBool32 bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool32 SetEvent(IntPtr hEvent);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool32 SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, SysBool32 fResume);

        [DllImport(LibraryName)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport(LibraryName)]
        public static extern uint WaitForMultipleObjects(uint nCount, IntPtr* lpHandles, SysBool32 bWaitAll, uint dwMilliseconds);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool32 CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetLastError();
    }
}