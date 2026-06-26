using System.Runtime.CompilerServices;

namespace ShioUI.Utils;

public static class GenericExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSuccessed(this RenderResult _this)
        => _this == RenderResult.Successed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldImmediatelyReturn(this RenderResult _this)
        => _this >= RenderResult.LayoutDesync;
}
