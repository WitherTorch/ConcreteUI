using System;
using System.Runtime.InteropServices;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct AppBarData
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Rect rc;
        public IntPtr lParam;
    }
}
