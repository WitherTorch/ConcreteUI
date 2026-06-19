using System;

using ConcreteUI.Windows;

namespace ConcreteUI;

public interface IWindowMessageFilter
{
    public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result);
}
