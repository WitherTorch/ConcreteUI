using WitherTorch.Common;

namespace ConcreteUI.Windows;

public interface IHwndOwner : IWindowMessageFilter, ICheckableDisposable
{
    public nint Handle { get; }
}
