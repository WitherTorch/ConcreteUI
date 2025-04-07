using System;
using System.Drawing;
using System.Windows.Forms;

using ConcreteUI.Graphics;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Controls
{
    partial class ContextMenu
    {
        public event EventHandler ItemClicked;
        public ContextMenuItem[] MenuItems { get; }
    }
}
