using System;

namespace ConcreteUI.Internals.Native
{
    [Flags]
    internal enum PreferredAppMode
    {
        Default,
        AllowDark,
        ForceDark,
        ForceLight,
        Max
    }
}