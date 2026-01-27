using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static class Gdi32
    {
        private const string LibraryName = "Gdi32.dll";

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern IntPtr CreateSolidBrush(uint color);

        [SuppressGCTransition]
        [DllImport(LibraryName)]
        public static extern bool DeleteObject(IntPtr handle);

        [DllImport(LibraryName)]
        public static extern int GetDeviceCaps(IntPtr hdc, int index);
    }
}
