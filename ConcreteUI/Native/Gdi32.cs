using System;
using System.Security;

namespace ConcreteUI.Native
{
    internal static partial class Gdi32
    {
        public static partial IntPtr CreateSolidBrush(uint color);
        public static partial bool DeleteObject(IntPtr handle);
        public static partial IntPtr GetDC(IntPtr hWnd);
        public static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        public static partial int GetDeviceCaps(IntPtr hdc, int index);
    }
}
