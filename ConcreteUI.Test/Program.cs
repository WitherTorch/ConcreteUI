using System;
using System.Windows.Forms;

namespace ConcreteUI.Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConcreteSettings.WindowMaterial = WindowMaterial.Acrylic;
            Application.Run(new MainWindow(null));
        }
    }
}