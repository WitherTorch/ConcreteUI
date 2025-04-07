using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Controls
{
    public interface IContainerElement : IDisposable
    {
        void AddChild(UIElement element);

        void AddChildren(params UIElement[] elements);

        void AddChildren(IEnumerable<UIElement> elements);

        void RemoveChild(UIElement element);

        IReadOnlyCollection<UIElement> Children { get; }

        UIElement FirstChild { get; }

        UIElement LastChild { get; }

        void RenderChildBackground(UIElement child, D2D1DeviceContext context);
    }
}
