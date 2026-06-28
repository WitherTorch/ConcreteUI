using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ShioUI.Layout;

namespace ShioUI.Internals;

internal sealed class UIElementEqualityComparer : IEqualityComparer<UIElement>
{
    public static readonly UIElementEqualityComparer Instance = new UIElementEqualityComparer();

    private UIElementEqualityComparer() { }

    public bool Equals(UIElement? x, UIElement? y) => ReferenceEquals(x, y);

    public int GetHashCode([DisallowNull] UIElement obj) => obj.ElementId;
}

internal sealed class LayoutNodeEqualityComparer : IEqualityComparer<LayoutNode>
{
    public static readonly LayoutNodeEqualityComparer Instance = new LayoutNodeEqualityComparer();

    private LayoutNodeEqualityComparer() { }

    public bool Equals(LayoutNode? x, LayoutNode? y) => ReferenceEquals(x, y);

    public int GetHashCode([DisallowNull] LayoutNode obj) => obj.NodeId;
}