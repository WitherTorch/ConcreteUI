namespace ConcreteUI.Input
{
    public enum IMECandicateStyle : uint
    {
        None = 0x0000,
        PositionedAtPoint = 0x0040,
        ExcludeRect = 0x0080,
    }

    public enum IMECompositionStyle : uint
    {
        Default = 0x0000,
        PositionedIntoRect = 0x0001,
        PositionedAtPoint = 0x0002,
        ForcedPositionedAtPoint = 0x0020,
    }
}