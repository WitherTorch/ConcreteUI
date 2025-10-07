using System;

namespace ConcreteUI.Native
{
    internal static partial class Gdi32
    {
        public static partial IntPtr CreateSolidBrush(uint color);
        public static partial bool DeleteObject(IntPtr handle);
        public static partial int GetDeviceCaps(IntPtr hdc, int index);
    }
}
