#if NET472_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class DwmApi
    {       
        private const string DWMAPI_DLL = "dwmapi.dll";

        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmExtendFrameIntoClientArea(IntPtr hWnd, Margins* pMargins);

        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern partial void DwmEnableBlurBehindWindow(IntPtr hwnd, DWMBlurBehind* blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern partial void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWMBlurBehind blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmIsCompositionEnabled(bool* enabled);

        [DllImport(DWMAPI_DLL)]
        public static extern partial bool DwmDefWindowProc(IntPtr hWnd, uint msg, nint wParam, nint lParam, nint* plResult);
    }
}
#endif
