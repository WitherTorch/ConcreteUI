using ConcreteUI.Controls;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class PageRectLayoutNode : LayoutNode
    {
        private readonly LayoutProperty _property;

        public PageRectLayoutNode(LayoutProperty property)
        {
            _property = property;
        }

        public override int Compute(in LayoutNodeManager manager)
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

        public override bool Equals(object? obj) => obj is PageRectLayoutNode another && _property == another._property;

        public override int GetHashCode() => _property.GetHashCode();
    }
}