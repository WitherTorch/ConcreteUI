using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class Shell32
    {
        private const string LibraryName = "shell32.dll";

        [DllImport(LibraryName)]
        public static extern IntPtr SHAppBarMessage(uint dwMessage, AppBarData* pData);
    }
}
