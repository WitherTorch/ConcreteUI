using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Winmm
    {
        private const string LibraryName = "winmm.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int timeBeginPeriod(uint period);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int timeEndPeriod(uint period);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern int timeGetDevCaps(TimeCapability* ptc, uint cbtc);
    }
}
