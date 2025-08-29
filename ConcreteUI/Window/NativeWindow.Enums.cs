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

    public enum DialogResult : uint
    {
        Invalid = 0,
        Ok = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
        Close = 8,
        Help = 9,
        TryAgain = 10,
        Continue = 11,
        Timeout = 32000
    }

}
