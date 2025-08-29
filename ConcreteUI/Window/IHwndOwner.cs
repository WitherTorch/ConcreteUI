using System;

namespace ConcreteUI.Window
{
    public interface IHwndOwner : IWindowMessageFilter, IDisposable
    {
        public nint Handle { get; }
    }
}
