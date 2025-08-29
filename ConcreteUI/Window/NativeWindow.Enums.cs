namespace ConcreteUI.Window
{
    public enum CloseReason : uint
    {
        Unknown = 0,
        Programmically,
        UserClicked,
    }

    public enum WindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2,
    }
}
