using System;

using ConcreteUI.Internals.Native;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class SystemParameters
    {
        private const uint SPI_GETKEYBOARDDELAY = 0x0016;
        private const uint SPI_GETKEYBOARDSPEED = 0x000A;
        private const float speedINTERVAL = (400f - 33f) / 31f;

        static unsafe SystemParameters()
        {
            int intVal = 0;
            User32.SystemParametersInfoW(SPI_GETKEYBOARDDELAY, 0, &intVal, 0);
            KeyboardDelay = intVal * 1000;
            IntPtr nativeIntVal = default;
            User32.SystemParametersInfoW(SPI_GETKEYBOARDSPEED, 0, &nativeIntVal, 0);
            intVal = nativeIntVal.ToInt32();
            switch (intVal)
            {
                case 0:
                    KeyboardSpeed = 400;
                    break;
                case 31:
                    KeyboardSpeed = 33;
                    break;
                default:
                    KeyboardSpeed = (int)(400f - intVal * speedINTERVAL);
                    break;
            }
            DoubleClickTime = unchecked((int)User32.GetDoubleClickTime());
        }

        public static int KeyboardDelay { get; }
        public static int KeyboardSpeed { get; }
        public static int DoubleClickTime { get; }
    }
}
