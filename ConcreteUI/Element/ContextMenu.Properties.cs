using System;

namespace ConcreteUI.Element;

partial class ContextMenu
{
    public event EventHandler? ItemClicked;

    public ContextMenuItem[] MenuItems { get; }
}
