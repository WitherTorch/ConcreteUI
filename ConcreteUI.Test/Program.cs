using System;

using ConcreteUI.Window2;

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
            ConcreteSettings.WindowMaterial = WindowMaterial.Acrylic;
            return MessageLoop.StartMessageLoop(new MainWindow(null));
        }

        private static void Window_Destroyed(object? sender, EventArgs e)
        {
            MessageLoop.StopMessageLoop();
        }
    }
}