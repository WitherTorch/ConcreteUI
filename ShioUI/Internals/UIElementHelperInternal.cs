using System;

using RiceTea.Core.Buffers;

using ShioUI.Windows;

namespace ShioUI.Internals;

internal static class UIElementHelperInternal
{
    public static ArrayPool<UIElement>.RentScope GetPathOfParentChain(CoreWindow window, UIElement element)
    {
        bool isDebugMode = ShioSettings.UseDebugMode;

        ArrayPool<UIElement> pool = ArrayPool<UIElement>.Shared;
        using PooledList<UIElement> list = new(pool, capacity: 1);
        list.Add(element);
        IElementContainer parent = element.Parent;
        while (!ReferenceEquals(parent, window))
        {
            if (parent is not UIElement parentElement || (isDebugMode && list.Contains(parentElement)))
            {
                InvalidOperationException.Throw();
                break;
            }
            list.Add(parentElement);
            parent = parent.Parent;
        }
        (UIElement[] buffer, int count) = list;
        return new ArrayPool<UIElement>.RentScope(pool, buffer, count);
    }
}
