using WitherTorch.Common;

namespace ConcreteUI.Window
{
    public interface IHwndOwner : IWindowMessageFilter, ICheckableDisposable
    {
        public nint Handle { get; }
    }
}
