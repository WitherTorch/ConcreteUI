using System;

namespace ConcreteUI.Graphics
{
    [Flags]
    public enum RenderingFlags : long
    {
        None = 0b0000,
        RedrawAll = 0b0001,
        Resize = 0b0010,
        ResizeTemporarily = _ResizeTemporarilyFlag | Resize,
        ResizeAndRedrawAll = Resize | RedrawAll,
        ResizeTemporarilyAndRedrawAll = ResizeTemporarily | RedrawAll,
        _FlagAllTrue = unchecked((long)ulong.MaxValue) & ~_ResizeTemporarilyFlag,
        _ResizeTemporarilyFlag = 0b0100,
    }
}
