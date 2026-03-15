using System;

using ConcreteUI.Internals.Native;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class SystemParameters
    {
        private const float SpeedInterval = (400f - 33f) / 31f;

        static unsafe SystemParameters()
        {
            const uint SPI_GETKEYBOARDDELAY = 0x0016;
            const uint SPI_GETKEYBOARDSPEED = 0x000A;

            nuint val = default;
            KeyboardDelay = User32.SystemParametersInfoW(SPI_GETKEYBOARDDELAY, 0, &val, 0) ? ((uint)val + 1) * 250 : 500;
            KeyboardSpeed = User32.SystemParametersInfoW(SPI_GETKEYBOARDSPEED, 0, &val, 0) ? val switch
            {
                0 => 400,
                31 => 33,
                _ => (uint)MathHelper.Max(400f - val * SpeedInterval, 0.0f),
            } : 400;
            DoubleClickTime = User32.GetDoubleClickTime();
        }

        public static uint KeyboardDelay { get; }
        public static uint KeyboardSpeed { get; }
        public static uint DoubleClickTime { get; }
    }
}
