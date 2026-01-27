using System;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Internals.Native
{
    internal static unsafe partial class DwmApi
    {
        public static partial int DwmExtendFrameIntoClientArea(IntPtr hWnd, Margins* pMargins);
        public static partial int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);
        public static partial int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, void* attrValue, int attrSize);
        public static partial void DwmEnableBlurBehindWindow(IntPtr hwnd, DWMBlurBehind* blurBehind);
        public static partial void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWMBlurBehind blurBehind);
        public static partial int DwmIsCompositionEnabled(bool* enabled);
        public static partial bool DwmDefWindowProc(IntPtr hWnd, uint msg, nint wParam, nint lParam, nint* plResult);

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
