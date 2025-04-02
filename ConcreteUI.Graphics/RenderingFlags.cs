using System;

namespace ConcreteUI.Graphics
{
    [Flags]
    public enum RenderingFlags : long
    {
        None = 0b000,
        RedrawAll = 0b001,
        Resize = 0b010,
        ResizeAndRedrawAll = Resize | RedrawAll,
        Locked = 0b100,
        _FlagAllTrue = unchecked((long)ulong.MaxValue)
    }
}
