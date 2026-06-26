using ShioUI.Internals.Native;

namespace ShioUI.Internals;

internal static class CustomWindowMessages
{
    public static readonly uint ShioWindowInvoke 
        = User32.RegisterWindowMessage(nameof(ShioWindowInvoke));

    public static readonly uint ShioDestroyWindowAsync 
        = User32.RegisterWindowMessage(nameof(ShioDestroyWindowAsync));

    public static readonly uint ShioUpdateRefreshRate 
        = User32.RegisterWindowMessage(nameof(ShioUpdateRefreshRate));
}
