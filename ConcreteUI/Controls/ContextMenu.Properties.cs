using System;

namespace ConcreteUI.Controls
{
    partial class ContextMenu
    {
        public event EventHandler? ItemClicked;
        public ContextMenuItem[] MenuItems { get; }
    }
}
