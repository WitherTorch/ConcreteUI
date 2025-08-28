using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static class Gdi32
    {
        private const string GDI32_DLL = "Gdi32.dll";

        [DllImport(GDI32_DLL)]
        public static extern IntPtr CreateSolidBrush(uint color);

        [DllImport(GDI32_DLL)]
        public static extern bool DeleteObject(IntPtr handle);
    }
}
