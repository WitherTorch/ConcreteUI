using System;
using System.Drawing;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NCCalcSizeParameters
    {
        public Rect rcNewWindow;
        public Rect rcOldWindow;
        public Rect rcClient;
        public WindowPosition* lppos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPosition
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public WindowPositionFlags flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WindowPlacement
    {
        public int Length;
        public int Flags;
        public ShowWindowCommands ShowCmd;
        public Point MinPosition;
        public Point MaxPosition;
        public Rect NormalPosition;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public AccentPolicy* Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public AccentState AccentState;
        public AccentFlags AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaintStruct
    {
        public IntPtr hdc;
        public bool fErase;
        public Rect rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        public int rgbReserved0;
        public int rgbReserved1;
        public int rgbReserved2;
        public int rgbReserved3;
    }
}
