using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace ShioUI.Layout;

// Just a specification for LayoutContext and VirtualLayoutContext, don't implement it manually!
public interface ILayoutContext
{
    Size PageSize { get; }
    ulong Timestamp { get; }

    VirtualLayoutContext.Builder CreateVirtualContextBuilder();
    LayoutContext.ChildrenEnumerator GetChildrenEnumerator(UIElement element);
    int GetComputedValue(LayoutNode node);
    int GetComputedValue(UIElement element, LayoutProperty property);
    int?[] GetComputedValues(UIElement element);
    LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property);
    bool TryGetParentElement(UIElement element, [NotNullWhen(true)] out UIElement? parent);
}