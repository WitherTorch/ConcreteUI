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
}