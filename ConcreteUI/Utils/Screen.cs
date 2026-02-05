using System;
using System.Drawing;

using ConcreteUI.Internals.Native;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Utils
{
    public static class Screen
    {
        public static unsafe bool TryGetScreenInfoFromHwnd(IntPtr hwnd, out ScreenInfo result)
        {
            if (hwnd == IntPtr.Zero)
                goto Failed;
            IntPtr monitor = User32.MonitorFromWindow(hwnd, MonitorFromWindowFlags.DefaultToNearest);
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

        public static unsafe bool TryGetBoundsCenteredScreen(IntPtr hwnd, out Rectangle bounds)
        {
            Rect rect;
            if (hwnd == IntPtr.Zero || !User32.GetWindowRect(hwnd, &rect))
            {
                bounds = Rectangle.Empty;
                return false;
            }
            return TryGetBoundsCenteredScreenCore(hwnd, rect.Size, out bounds);
        }

        public static unsafe bool TryGetBoundsCenteredScreen(IntPtr hwnd, Size size, out Rectangle bounds)
        {
            if (hwnd == IntPtr.Zero)
            {
                bounds = Rectangle.Empty;
                return false;
            }
            return TryGetBoundsCenteredScreenCore(hwnd, size, out bounds);
        }

        private static unsafe bool TryGetBoundsCenteredScreenCore(IntPtr hwnd, Size size, out Rectangle bounds)
        {
            if (!TryGetScreenInfoFromHwnd(hwnd, out ScreenInfo info))
            {
                bounds = Rectangle.Empty;
                return false;
            }
            Rect screenBounds = info.Bounds;
            bounds = new Rectangle(
                x: screenBounds.X + ((screenBounds.Width - size.Width) / 2),
                y: screenBounds.Y + ((screenBounds.Height - size.Height) / 2),
                height: size.Height,
                width: size.Width);
            return true;
        }
    }

    public readonly record struct ScreenInfo(
        bool IsPrimary,
        Rect Bounds,
        Rect WorkingArea);
}
