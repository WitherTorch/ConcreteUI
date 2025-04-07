using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class DwmApi
    {
        private const string DWMAPI_DLL = "dwmapi.dll";

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, Margins* pMargins);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, DWMBlurBehind* blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWMBlurBehind blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmIsCompositionEnabled(bool* enabled);

        [SuppressUnmanagedCodeSecurity, DllImport(DWMAPI_DLL)]
        public static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr* plResult);

    }
}
