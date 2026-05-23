using System.Runtime.InteropServices;

using ConcreteUI.Controls;

namespace ConcreteUI.Layout
{
    public abstract class UIElementDependedNode<T> : LayoutNode where T : UIElement
    {
        private readonly GCHandle _handle;

        protected UIElementDependedNode(T element) => _handle = GCHandle.Alloc(element, GCHandleType.Weak);

        public override int Compute(in LayoutNodeManager manager)
        {
            if (_handle.Target is not T element)
                return 0;
            return Compute(element, manager);
        }

        protected abstract int Compute(T element, in LayoutNodeManager manager);

        ~UIElementDependedNode() => _handle.Free();
    }
}
