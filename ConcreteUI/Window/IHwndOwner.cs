using WitherTorch.Common;

namespace ConcreteUI.Window
{
    public interface IHwndOwner : IWindowMessageFilter, ISafeDisposable
    {
        public nint Handle { get; }
    }
}
