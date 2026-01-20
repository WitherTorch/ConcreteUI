using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics;

using InlineMethod;

using WitherTorch.Common;

namespace ConcreteUI.Controls
{
    public interface IElementContainer : ISafeDisposable
    {
        IEnumerable<UIElement> GetElements();

        IEnumerable<UIElement> GetActiveElements()
#if NET8_0_OR_GREATER
            => ElementContainerDefaults.GetActiveElements(this);
#else
            ;
#endif

        void RenderBackground(UIElement element, in RegionalRenderingContext context);

#if NET8_0_OR_GREATER
        void ISafeDisposable.DisposeCore(bool disposing) => ElementContainerDefaults.DisposeCore(this, disposing);
#endif
    }

    public static class ElementContainerDefaults
    {
        [Inline(InlineBehavior.Keep, export: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<UIElement> GetActiveElements<T>(T container) where T : IElementContainer
            => container.GetElements();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeCore<T>(T container, bool disposing) where T : IElementContainer
        {
            if (!disposing)
                return;
            foreach (UIElement element in container.GetElements())
            {
                if (element is not IDisposable disposableElement)
                    continue;
                disposableElement.Dispose();
            }
        }
    }
}
