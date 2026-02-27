using System;
using System.Drawing;
using System.Runtime.InteropServices;

using ConcreteUI.Window;

using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NCCalcSizeParameters
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
    internal unsafe struct WindowPlacement
    {
        public int Length;
        public int Flags;
        public ShowWindowCommands ShowCmd;
        public Point MinPosition;
        public Point MaxPosition;
        public Rect NormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public void* Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public AccentFlags AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WindowClassEx
    {
        public uint cbSize;
        public ClassStyles style;
        public void* lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public void* lpszMenuName;
        public void* lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PumpingMessage
    {
        public IntPtr hwnd;
        public WindowMessage message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public Point pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        public uint cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct PaintStruct
    {
        public IntPtr hdc;
        public SysBool fErase;
        public Rect rcPaint;
        public SysBool fRestore;
        public SysBool fIncUpdate;
        public fixed byte rgbReserved[32];
    }
}
