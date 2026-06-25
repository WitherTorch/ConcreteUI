using System;

namespace ConcreteUI.Graphics;

[Flags]
public enum RenderingFlags : ulong
{
    // Base states
    None = 0b00000,
    BaseFlag = 0x00001,

    // Single states
    RedrawAll = BaseFlag | RedrawAllFlag,
    Resize = BaseFlag | ResizeFlag,
    ResizeTemporarily = BaseFlag | ResizeTemporarilyFlag,

    // Combinations
    ResizeAndRedrawAll = Resize | RedrawAll,
    ResizeTemporarilyAndRedrawAll = RedrawAll | ResizeTemporarily,

    // Flags
    RedrawAllFlag = 0b00010,
    ResizeFlag = 0b00100,
    ResizeTemporarilyFlag = ResizeFlag | _ResizeTemporarilyFlag_Standalone,

    // Special
    _ResizeTemporarilyFlag_Standalone = 0b01000,
    _FlagAllTrue = ulong.MaxValue & ~_ResizeTemporarilyFlag_Standalone,
}
