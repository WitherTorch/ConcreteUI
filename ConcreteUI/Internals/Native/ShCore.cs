using System;

namespace ConcreteUI.Internals.Native
{
    internal static partial class ShCore
    {
        public static partial int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
    }
}
