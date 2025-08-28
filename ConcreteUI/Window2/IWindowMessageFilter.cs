using ConcreteUI.Native;

namespace ConcreteUI.Window2
{
    public interface IWindowMessageFilter
    {
        public bool TryProcessWindowMessage(WindowMessage message, nint wParam, nint lParam, out nint result);
    }
}
