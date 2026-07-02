using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShioUI.Utils;

[StructLayout(LayoutKind.Auto)]
public readonly struct RecalculateLayoutInformation
{
    public readonly ulong LayoutTimestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RecalculateLayoutInformation(ulong layoutTimestamp)
    {
        LayoutTimestamp = layoutTimestamp;
    }
}
