using System.Runtime.InteropServices;

namespace ShioUI.Layout;

public abstract class UIElementDependedNode<T> : LayoutNode where T : UIElement
{
    private readonly GCHandle _handle;

    protected UIElementDependedNode(T element) => _handle = GCHandle.Alloc(element, GCHandleType.Weak);

    protected override int ComputeCore(in LayoutContext context)
    {
        if (_handle.Target is not T element)
            return 0;
        return ComputeCore(element, context);
    }

    protected abstract int ComputeCore(T element, in LayoutContext context);

    ~UIElementDependedNode() => _handle.Free();
}
