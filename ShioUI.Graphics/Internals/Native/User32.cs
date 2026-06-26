using System;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Structures;

namespace ShioUI.Graphics.Internals.Native;

[SuppressUnmanagedCodeSecurity]
internal static unsafe class User32
{
    private const string LibraryName = "user32.dll";

    [DllImport(LibraryName)]
    public static extern SysBool32 GetClientRect(IntPtr hWnd, Rect* lpRect);
}
