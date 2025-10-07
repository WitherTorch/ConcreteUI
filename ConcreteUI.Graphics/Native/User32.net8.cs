#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Security;

namespace ConcreteUI.Graphics.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe partial class User32
    {
        private const string USER32_DLL = "user32.dll";

        [SuppressGCTransition]
        [DllImport(USER32_DLL)]
        public static extern partial bool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);
    }
}
#endif