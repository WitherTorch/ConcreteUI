#if NET8_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class DwmApi
    {
        private const string DWMAPI_DLL = "dwmapi.dll";

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmExtendFrameIntoClientArea(IntPtr hWnd, Margins* pMargins);

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial void DwmEnableBlurBehindWindow(IntPtr hwnd, DWMBlurBehind* blurBehind);

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWMBlurBehind blurBehind);

        [SuppressGCTransition]
        [DllImport(DWMAPI_DLL)]
        public static extern partial int DwmIsCompositionEnabled(bool* enabled);

        [DllImport(DWMAPI_DLL)]
        public static extern partial bool DwmDefWindowProc(IntPtr hWnd, uint msg, nint wParam, nint lParam, nint* plResult);
    }
}
#endif
