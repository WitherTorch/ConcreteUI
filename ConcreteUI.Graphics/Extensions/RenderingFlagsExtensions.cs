using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Graphics.Extensions;

public static class RenderingFlagsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRenderRequest(this RenderingFlags renderingFlags) 
        => renderingFlags.HasFlagFast(RenderingFlags.BaseFlag);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRedrawAll(this RenderingFlags renderingFlags) 
        => renderingFlags.HasFlagFast(RenderingFlags.RedrawAll);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasResize(this RenderingFlags renderingFlags) 
        => renderingFlags.HasFlagFast(RenderingFlags.Resize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasResizeTemporarily(this RenderingFlags renderingFlags) 
        => renderingFlags.HasFlagFast(RenderingFlags.ResizeTemporarily);
}
