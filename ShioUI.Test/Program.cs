using System;

namespace ShioUI.Test;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static int Main()
    {
        ShioSettings.WindowMaterial = WindowMaterial.Default;
        ShioSettings.UseDebugMode = false;
        return WindowMessageLoop.Start(new MainWindow(null));
    }

    private static void Window_Destroyed(object? sender, EventArgs e)
    {
        WindowMessageLoop.Stop();
    }
}