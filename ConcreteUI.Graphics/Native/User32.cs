using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class User32
    {
        private const string USER32_DLL = "user32.dll";

        [DllImport(USER32_DLL)]
        public static extern bool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);
    }
}
