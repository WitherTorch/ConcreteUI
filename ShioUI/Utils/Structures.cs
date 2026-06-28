using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShioUI.Utils;

[StructLayout(LayoutKind.Auto)]
public readonly struct RecalculateLayoutInformation
{
    public readonly ulong LayoutTimestamp;
    public readonly bool ClearCache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RecalculateLayoutInformation(ulong layoutTimestamp, bool clearCache)
    {
        LayoutTimestamp = layoutTimestamp;
        ClearCache = clearCache;
    }
}
