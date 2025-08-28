using System;

namespace ConcreteUI.Window2
{
    public interface IHwndOwner : IWindowMessageFilter, IDisposable
    {
        public nint Handle { get; }
    }
}
