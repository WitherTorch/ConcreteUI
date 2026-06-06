using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
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

    public interface ICoordinateTranslator
    {
        Point PageToWindow(UIElement element, Point point);

        PointF PageToWindow(UIElement element, PointF point);

        Point WindowToPage(UIElement element, Point point);

        PointF WindowToPage(UIElement element, PointF point);
    }

    public static class ElementContainerDefaults
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<UIElement?> GetActiveElements<T>(T container) where T : IElementContainer
            => container.GetElements();
    }
}
