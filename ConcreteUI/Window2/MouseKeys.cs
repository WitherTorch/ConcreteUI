using System;

namespace ConcreteUI.Window2
{
    [Flags]
    public enum MouseKeys : uint
    {
        None = 0x0000,
        ControlKey = 0x0008,
        LeftButton = 0x0001,
        MiddleButton = 0x0010,
        RightButton = 0x0002,
        ShiftKey = 0x0004,
        XButton1 = 0x0020,
        XButton2 = 0x0040
    }
}
