using System;

namespace ConcreteUI.Controls
{
    partial class ContextMenu
    {
        public event EventHandler? ItemClicked;

        public bool IsDisposed => _disposed;
        public ContextMenuItem[] MenuItems { get; }
    }
}
