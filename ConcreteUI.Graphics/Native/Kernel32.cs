using System;
using System.Runtime.CompilerServices;
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
    }
}
