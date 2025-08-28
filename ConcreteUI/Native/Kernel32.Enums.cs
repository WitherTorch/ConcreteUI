using System;

namespace ConcreteUI.Native
{
    [Flags]
    public enum GlobalAllocFlags : uint
    {
        Fixed = 0x0000,
        Movable = 0x0002,
        ZeroInit = 0x0040,

        StandardHandle = Movable | ZeroInit,
        StandardPointer = Fixed | ZeroInit
    }
}
