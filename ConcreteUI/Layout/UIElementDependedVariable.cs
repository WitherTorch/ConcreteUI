using System.Runtime.InteropServices;

using ConcreteUI.Controls;

namespace ConcreteUI.Layout
{
    public abstract class UIElementDependedVariable<T> : LayoutVariable where T : UIElement
    {
        private readonly GCHandle _handle;

        protected UIElementDependedVariable(T element) => _handle = GCHandle.Alloc(element, GCHandleType.Weak);

        public override int Compute(in LayoutVariableManager manager)
        {
            if (_handle.Target is not T element)
                return 0;
            return Compute(element, manager);
        }

        protected abstract int Compute(T element, in LayoutVariableManager manager);

        ~UIElementDependedVariable() => _handle.Free();
    }
}
