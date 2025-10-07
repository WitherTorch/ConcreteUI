namespace ConcreteUI.Graphics.Native
{
    internal static unsafe partial class User32
    {
        public static partial bool EnumDisplaySettingsW(char* lpszDeviceName, int iModeNum, DeviceModeW* lpDevMode);
    }
}
