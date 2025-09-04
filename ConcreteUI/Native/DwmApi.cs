using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class DwmApi
    {
        private const string DWMAPI_DLL = "dwmapi.dll";

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, Margins* pMargins);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);

        [DllImport(DWMAPI_DLL)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, DWMBlurBehind* blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWMBlurBehind blurBehind);

        [DllImport(DWMAPI_DLL)]
        public static extern int DwmIsCompositionEnabled(bool* enabled);

        [DllImport(DWMAPI_DLL)]
        public static extern bool DwmDefWindowProc(IntPtr hWnd, uint msg, nint wParam, nint lParam, nint* plResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DwmGetWindowAttribute<T>(IntPtr hwnd, DwmWindowAttribute attr, out T value) where T : unmanaged
        {
            int hr = DwmGetWindowAttribute(hwnd, attr, UnsafeHelper.AsPointerOut(out value), sizeof(T));
            return hr >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DwmGetWindowAttributeOrDefault<T>(IntPtr hwnd, DwmWindowAttribute attr, T defaultValue = default) where T : unmanaged
            => DwmGetWindowAttribute(hwnd, attr, out T result) ? result : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DwmSetWindowAttribute<T>(IntPtr hwnd, DwmWindowAttribute attr, in T value) where T : unmanaged
            => DwmSetWindowAttribute(hwnd, attr, UnsafeHelper.AsPointerIn(in value), sizeof(T));
    }
}
