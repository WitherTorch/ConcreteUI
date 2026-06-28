using System.Collections.Generic;
using System.Runtime.CompilerServices;

using InlineMethod;

using ShioUI.Graphics;
using ShioUI.Windows;

namespace ShioUI;

public interface IElementContainer
{
    IElementContainer Parent { get; }

    IRenderWindow Window { get; }

    CoreWindow RootWindow { get; }

    bool IsBackgroundOpaque(UIElement element);

    IEnumerable<UIElement?> GetElements();

    IEnumerable<UIElement?> GetActiveElements()
#if NET8_0_OR_GREATER
        => ElementContainerDefaults.GetActiveElements(this);
#else
        ;
#endif

    void RenderBackground(UIElement element, in RegionalRenderingContext context);
}

public static class ElementContainerDefaults
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<UIElement?> GetActiveElements<T>(T container) where T : IElementContainer
        => container.GetElements();
}
