using System;
using System.Drawing;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AppBarData
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Rect rc;
        public IntPtr lParam;
    }
}
