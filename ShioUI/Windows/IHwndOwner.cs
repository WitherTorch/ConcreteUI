using RiceTea.Core;

namespace ShioUI.Windows;

public interface IHwndOwner : IWindowMessageFilter, ICheckableDisposable
{
    public nint Handle { get; }
}
