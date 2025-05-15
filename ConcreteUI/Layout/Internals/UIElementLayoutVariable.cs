using System;

using ConcreteUI.Controls;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class UIElementLayoutVariable : LayoutVariable
    {
        private readonly WeakReference<UIElement> _reference;
        private readonly LayoutProperty _property;

        public UIElementLayoutVariable(UIElement element, LayoutProperty property)
        {
            _reference = new WeakReference<UIElement>(element);
            if (property < LayoutProperty.Left || property >= LayoutProperty._Last)
                throw new ArgumentOutOfRangeException(nameof(property));
            _property = property;
        }

        public override int Compute(in LayoutVariableManager manager)
        {
            if (!_reference.TryGetTarget(out UIElement? element))
                return 0;
            return manager.GetComputedValue(element, _property);
        }

        public override bool Equals(object? obj) => obj is UIElementLayoutVariable another && Equals(another);

        private bool Equals(UIElementLayoutVariable another)
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
}
