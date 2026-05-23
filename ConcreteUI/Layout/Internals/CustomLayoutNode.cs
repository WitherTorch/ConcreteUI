namespace ConcreteUI.Layout.Internals
{
    internal sealed class CustomLayoutNode : LayoutNode
    {
        private readonly CustomComputeDelegate _func;

        public CustomLayoutNode(CustomComputeDelegate func)
        {
            _func = func;
        }

        public override int Compute(in LayoutNodeManager manager) => _func.Invoke(in manager);

        public override bool Equals(object? obj) => obj is CustomLayoutNode another && _func == another._func;

        public override int GetHashCode() => _func.GetHashCode();
    }
}