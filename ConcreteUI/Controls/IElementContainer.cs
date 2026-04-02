using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;

using InlineMethod;

namespace ConcreteUI.Controls
{
    public interface IElementContainer
    {
        bool IsBackgroundOpaque(UIElement element);

        IEnumerable<UIElement?> GetElements();

        IEnumerable<UIElement?> GetActiveElements()
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.GetActiveElements(this);
#else
            ;
#endif

        void RenderBackground(UIElement element, in RegionalRenderingContext context);

        Point PointToGlobal(UIElement element, Point point)
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.PointToGlobal(element, point);
#else
            ;
#endif

        PointF PointToGlobal(UIElement element, PointF point)
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.PointToGlobal(element, point);
#else
            ;
#endif

        Point PointToLocal(UIElement element, Point point)
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.PointToLocal(element, point);
#else
            ;
#endif

        PointF PointToLocal(UIElement element, PointF point)
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.PointToLocal(element, point);
#else
            ;
#endif
    }

    public static class ElementContainerDefaults
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static IEnumerable<UIElement?> GetActiveElements<T>(T container) where T : IElementContainer
            => container.GetElements();

        [Inline(InlineBehavior.Keep, export: true)]
        public static Point PointToGlobal(UIElement element, Point point) => GraphicsUtils.PointToGlobal(element.Location, point);

        [Inline(InlineBehavior.Keep, export: true)]
        public static PointF PointToGlobal(UIElement element, PointF point) => GraphicsUtils.PointToGlobal(element.Location, point);

        [Inline(InlineBehavior.Keep, export: true)]
        public static Point PointToLocal(UIElement element, Point point) => GraphicsUtils.PointToLocal(element.Location, point);

        [Inline(InlineBehavior.Keep, export: true)]
        public static PointF PointToLocal(UIElement element, PointF point) => GraphicsUtils.PointToLocal(element.Location, point);
    }
}
