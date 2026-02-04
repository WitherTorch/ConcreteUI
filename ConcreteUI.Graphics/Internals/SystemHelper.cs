using System;

namespace ConcreteUI.Graphics.Internals
{
    internal static class SystemHelper
    {
        private static readonly bool _isWindows8OrHigher = CheckIsWindows8OrHigher();
        private static readonly bool _isWindows8_1OrHigher = CheckIsWindows8_1OrHigher();

        public static bool IsWindows8OrHigher => _isWindows8OrHigher;

        public static bool IsWindows8_1OrHigher => _isWindows8_1OrHigher;

        private static bool CheckIsWindows8OrHigher()
        {
#if NET8_0_OR_GREATER
            return OperatingSystem.IsWindowsVersionAtLeast(6, 2);
#else
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return false;
            Version version = osVer.Version;
            return version >= new Version(6, 2);
#endif
        }

        private static bool CheckIsWindows8_1OrHigher()
        {
#if NET8_0_OR_GREATER
            return OperatingSystem.IsWindowsVersionAtLeast(6, 3);
#else
            OperatingSystem osVer = Environment.OSVersion;
            if (osVer.Platform != PlatformID.Win32NT)
                return false;
            Version version = osVer.Version;
            return version >= new Version(6, 3);
#endif
        }
    }
}
