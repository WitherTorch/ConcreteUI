using System;

namespace ConcreteUI.Graphics.Internals
{
    internal static class SystemHelper
    {
        private static readonly bool _isWindows8OrHigher = CheckIsWindows8OrHigher();

        public static bool IsWindows8OrHigher => _isWindows8OrHigher;

        private static bool CheckIsWindows8OrHigher()
        {
#if NET8_0_OR_GREATER
            if (!OperatingSystem.IsWindows())
                return false;
            OperatingSystem osVer = Environment.OSVersion;
#else
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return false;
#endif
            Version version = osVer.Version;
            return version >= new Version(6, 2);
        }
    }
}
