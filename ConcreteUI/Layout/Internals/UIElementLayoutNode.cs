using System;

namespace ConcreteUI.Layout.Internals;

internal sealed class UIElementLayoutNode : LayoutNode
{
    private readonly WeakReference<UIElement> _reference;
    private readonly LayoutProperty _property;

    public UIElementLayoutNode(UIElement element, LayoutProperty property)
    {
        if (property < LayoutProperty.Left || property >= LayoutProperty._Last)
            throw new ArgumentOutOfRangeException(nameof(property));
        _property = property;
        _reference = new WeakReference<UIElement>(element);
    }

    public UIElementLayoutNode(WeakReference<UIElement> reference, LayoutProperty property)
    {
        if (property < LayoutProperty.Left || property >= LayoutProperty._Last)
            throw new ArgumentOutOfRangeException(nameof(property));
        _property = property;
        _reference = reference;
    }

    public override int Compute(in LayoutNodeManager manager)
    {
        if (!_reference.TryGetTarget(out UIElement? element))
            return 0;
        return manager.GetComputedValue(element, _property);
    }

    public override bool Equals(object? obj) => obj is UIElementLayoutNode another && Equals(another);

    private bool Equals(UIElementLayoutNode another)
    {
        if (!_reference.TryGetTarget(out UIElement? element))
            element = null;
        if (!another._reference.TryGetTarget(out UIElement? anotherElement))
            anotherElement = null;
        return element == anotherElement && _property == another._property;
    }

    public override int GetHashCode()
    {
        if (_reference.TryGetTarget(out UIElement? element))
            return element.GetHashCode() ^ _property.GetHashCode();
        return _property.GetHashCode();
    }
}
