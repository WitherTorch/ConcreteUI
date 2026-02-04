using System;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class User32
    {
        private const string LibraryName = "user32.dll";

        [DllImport(LibraryName)]
        public static extern bool GetClientRect(IntPtr hWnd, Rect* lpRect);
    }
}
