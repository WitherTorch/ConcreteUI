using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class NtDll
    {
        private const string LibraryName = "ntdll.dll";

        [DllImport(LibraryName)]
        public static extern uint NtDelayExecution(SysBool alertable, long* delayInterval);

        [DllImport(LibraryName)]
        public static extern uint NtSetTimerResolution(uint desiredTime, SysBool setResolution, uint* actualTime);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NtSetTimerResolution(uint desiredTime, SysBool setResolution) => NtSetTimerResolution(desiredTime, setResolution, &desiredTime);
    }
}
