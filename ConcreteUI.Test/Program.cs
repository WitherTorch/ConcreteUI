using System;

namespace ConcreteUI.Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            ConcreteSettings.WindowMaterial = WindowMaterial.Default;
            return WindowMessageLoop.Start(new MainWindow(null));
        }

        private static void Window_Destroyed(object? sender, EventArgs e)
        {
            WindowMessageLoop.Stop();
        }
    }
}