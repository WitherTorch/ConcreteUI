using System;

namespace ConcreteUI.Layout.Internals
{
    internal sealed class CustomLayoutNode : LayoutNode
    {
        private readonly Func<LayoutNodeManager, int> _func;

        public CustomLayoutNode(Func<LayoutNodeManager, int> func)
        {
            _func = func;
        }

        public override int Compute(in LayoutNodeManager manager) => _func(manager);

        public override bool Equals(object? obj) => obj is CustomLayoutNode another && _func == another._func;

        public override int GetHashCode() => _func.GetHashCode();
    }
}