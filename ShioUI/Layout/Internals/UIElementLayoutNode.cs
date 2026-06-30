using System;
using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class UIElementLayoutNode : UIElementReferencedNode<UIElement>
{
    private readonly LayoutProperty _property;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UIElementLayoutNode(WeakReference<UIElement> reference, LayoutProperty property) : base(reference)
    {
        if (property < LayoutProperty.Left || property >= LayoutProperty._Last)
            ArgumentOutOfRangeException.Throw(nameof(property));
        _property = property;
    }

    protected override int ComputeCore(UIElement element, in LayoutContext context) 
        => context.GetComputedValue(element, _property);
}
