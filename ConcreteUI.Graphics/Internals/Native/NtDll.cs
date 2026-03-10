using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class NtDll
    {
        private const string LibraryName = "ntdll.dll";

        [DllImport(LibraryName)]
        public static extern uint NtSetTimerResolution(uint desiredTime, SysBool32 setResolution, uint* actualTime);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NtSetTimerResolution(uint desiredTime, SysBool32 setResolution) => NtSetTimerResolution(desiredTime, setResolution, &desiredTime);
    }
}
