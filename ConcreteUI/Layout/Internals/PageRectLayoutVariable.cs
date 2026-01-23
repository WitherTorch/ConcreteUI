using ConcreteUI.Controls;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class PageRectLayoutVariable : LayoutVariable
    {
        private readonly LayoutProperty _property;

        public PageRectLayoutVariable(LayoutProperty property)
        {
            _property = property;
        }

        public override int Compute(in LayoutVariableManager manager)
        {
            Rect rect = manager.GetPageRect();
            return _property switch
            {
                LayoutProperty.Left => rect.Left,
                LayoutProperty.Top => rect.Top,
                LayoutProperty.Right => rect.Right,
                LayoutProperty.Bottom => rect.Bottom,
                LayoutProperty.Width => rect.Width,
                LayoutProperty.Height => rect.Height,
                _ => 0
            };
        }

        public override bool Equals(object? obj) => obj is PageRectLayoutVariable another && _property == another._property;

        public override int GetHashCode() => _property.GetHashCode();
    }
}