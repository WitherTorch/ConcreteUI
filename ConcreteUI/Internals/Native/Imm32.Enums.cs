using System;

namespace ConcreteUI.Internals.Native
{
    [Flags]
    internal enum ImmAssociateContextEx_Flags
    {
        Children = 0x0001,
        Default = 0x0010,
        IgnoreNoContext = 0x0020
    }

    /// <summary>
    /// bit field for IMC_SETCOMPOSITIONWINDOW, IMC_SETCANDIDATEWINDOW
    /// </summary>
    [Flags]
    public enum InputMethodCFlags : uint
    {
        Default = 0x0000,
        Rect = 0x0001,
        Point = 0x0002,
        ForcePosition = 0x0020,
        CandicatePosition = 0x0040,
        Exclude = 0x0080,
    }
}