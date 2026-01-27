#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static partial class Gdi32
    {
        private const string GDI32_DLL = "Gdi32.dll";

        [SuppressGCTransition]
        [DllImport(GDI32_DLL)]
        public static extern partial IntPtr CreateSolidBrush(uint color);

        [SuppressGCTransition]
        [DllImport(GDI32_DLL)]
        public static extern partial bool DeleteObject(IntPtr handle);

        [DllImport(GDI32_DLL)]
        public static extern partial int GetDeviceCaps(IntPtr hdc, int index);
    }
}
#endif