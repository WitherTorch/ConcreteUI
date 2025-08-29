using System;

using ConcreteUI.Native;

namespace ConcreteUI
{
    public interface IWindowMessageFilter
    {
        public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result);
    }
}
