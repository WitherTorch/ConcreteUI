using System;

namespace ShioUI.Internals.Native;

[Flags]
internal enum PreferredAppMode
{
    Default,
    AllowDark,
    ForceDark,
    ForceLight,
    Max
}