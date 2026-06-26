using System;
using System.Runtime.InteropServices;

using RiceTea.Core.Structures;

namespace ShioUI.Internals.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct AppBarData
{
    public int cbSize;
    public IntPtr hWnd;
    public uint uCallbackMessage;
    public uint uEdge;
    public Rect rc;
    public IntPtr lParam;
}
