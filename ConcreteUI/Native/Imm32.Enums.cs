using System;

namespace ConcreteUI.Native
{
    [Flags]
    public enum ImmAssociateContextEx_Flags
    {
        Children = 0x0001,
        Default = 0x0010,
        IgnoreNoContext = 0x0020
    }

    [Flags]
    public enum IMECompositionFlags
    {
        CompositionReadingString = 0x0001,
        CompositionReadingAttribute = 0x0002,
        CompositionReadingClause = 0x0004,
        CompositionString = 0x0008,
        CompositionAttribute = 0x0010,
        CompositionClause = 0x0020,
        CursorPosition = 0x0080,
        DeltaStart = 0x0100,
        ResultReadString = 0x0200,
        ResultReadClause = 0x0400,
        ResultString = 0x0800,
        ResultClause = 0x1000,
        Control_InsertCharacter = 0x2000,
        Control_NoMoveCaret = 0x4000
    }
}
