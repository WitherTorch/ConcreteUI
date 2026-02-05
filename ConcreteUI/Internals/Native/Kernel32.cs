using System;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class Kernel32
    {
        private const string LibraryName = "kernel32.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern uint GetCurrentThreadId();

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr GetModuleHandleW(char* lpModuleName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr GlobalAlloc(GlobalAllocFlags uFlags, nuint dwBytes);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport(LibraryName)]
        public static extern void* GlobalLock(IntPtr hMem);

        [DllImport(LibraryName)]
        public static extern SysBool GlobalUnlock(IntPtr hMem);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateWaitableTimerW(void* lpTimerAttributes, SysBool bManualReset, char* lpTimerName);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool SetWaitableTimer(IntPtr hTimer, long* lpDueTime, nint lPeriod, void* pfnCompletionRoutine, void* lpArgToCompletionRoutine, SysBool fResume);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern SysBool CloseHandle(IntPtr hObject);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int GetLastError();
    }
}
