using System;

namespace ConcreteUI.Internals
{
    internal static class SystemConstants
    {
        public static readonly SystemVersionLevel VersionLevel = GetSystemVersionLevel();

        private static SystemVersionLevel GetSystemVersionLevel()
        {
            Version OSVersion = Environment.OSVersion.Version;
            int major = OSVersion.Major;
            if (major >= 10)
            {
                int build = OSVersion.Build;
                if (build >= 22621) // Windows 11 (NT 10.0, 10.0.22621.x)
                    return SystemVersionLevel.Windows_11_After;
                if (build >= 22000) // Windows 11 (NT 10.0, 10.0.22000.x)
                    return SystemVersionLevel.Windows_11_21H2;
                if (build >= 18362)
                    return SystemVersionLevel.Windows_10_19H1;
                if (build >= 17134) // Windows 10 RS4 (NT 10.0, 10.0.17134.x)
                    return SystemVersionLevel.Windows_10_Redstone_4;
                return SystemVersionLevel.Windows_10;
            }
            if (major == 6)
            {
                int minor = OSVersion.Minor;
                switch (minor)
                {
                    case 4:
                    case 3:
                    case 2: // Windows 8 (NT 6.2) & 8.1 (NT 6.3) & 10 Beta (NT 6.4)
                        return SystemVersionLevel.Windows_8;
                    case 1: // Windows 7 (NT 6.1)
                        return SystemVersionLevel.Windows_7;
                    default:
                        break;
                }
            }
            return SystemVersionLevel.Unknown;
        }
    }
}
