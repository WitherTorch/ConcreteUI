using System;

using ConcreteUI.Native;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Window2
{
    public static class Screen
    {
        public static unsafe bool TryGetScreenInfoFromHwnd(IntPtr hwnd, out ScreenInfo result)
        {
            if (hwnd == IntPtr.Zero)
                goto Failed;
            IntPtr monitor = User32.MonitorFromWindow(hwnd, GetMonitorFlags.DefaultToNearest);
            if (monitor == IntPtr.Zero)
                goto Failed;

            MonitorInfo info = new MonitorInfo() { cbSize = UnsafeHelper.SizeOf<MonitorInfo>() };
            if (!User32.GetMonitorInfoW(monitor, &info))
                goto Failed;

            const uint MONITORINFOF_PRIMARY = 0x00000001;

            result = new ScreenInfo(
                IsPrimary: (info.dwFlags & MONITORINFOF_PRIMARY) == MONITORINFOF_PRIMARY,
                Bounds: info.rcMonitor,
                WorkingArea: info.rcWork);
            return true;

        Failed:
            result = default;
            return false;
        }
    }

    public readonly record struct ScreenInfo(
        bool IsPrimary,
        Rect Bounds,
        Rect WorkingArea);
}
