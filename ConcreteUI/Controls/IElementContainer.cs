using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

namespace ConcreteUI.Controls
{
    public interface IElementContainer
    {
        IRenderer GetRenderer();

        CoreWindow GetWindow();

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
        public static IEnumerable<UIElement?> GetActiveElements<T>(T container) where T : IElementContainer
            => container.GetElements();
    }
}
