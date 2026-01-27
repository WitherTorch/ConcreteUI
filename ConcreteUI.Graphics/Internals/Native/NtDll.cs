using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class NtDll
    {
        private const string LibraryName = "ntdll.dll";

        [DllImport(LibraryName)]
        public static extern uint NtDelayExecution(bool alertable, long delayInterval);
    }
}
