using System;

namespace ShioUI.Layout;

public abstract class UIElementReferencedNode<T> : LayoutNode where T : UIElement
{
    private readonly WeakReference<T> _reference;

    protected UIElementReferencedNode(WeakReference<T> reference) => _reference = reference;

    public override int Compute(in LayoutNodeManager manager)
    {
        if (!_reference.TryGetTarget(out T? element))
            return 0;
        return Compute(element, manager);
    }

    protected abstract int Compute(T element, in LayoutNodeManager manager);
}
