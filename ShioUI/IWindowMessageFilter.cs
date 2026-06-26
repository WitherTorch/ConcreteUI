using System;

using ShioUI.Windows;

namespace ShioUI;

public interface IWindowMessageFilter
{
    public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result);
}
