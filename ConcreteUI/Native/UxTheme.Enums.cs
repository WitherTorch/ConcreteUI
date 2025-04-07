using System;

namespace ConcreteUI.Native
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