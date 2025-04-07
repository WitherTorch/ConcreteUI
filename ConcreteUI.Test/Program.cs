using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace ConcreteUI.Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        [HandleProcessCorruptedStateExceptions]
        static void Main()
        {
            ConcreteSettings.WindowMaterial = WindowMaterial.Acrylic;
                Application.Run(new MainWindow(null));
        }
    }
}