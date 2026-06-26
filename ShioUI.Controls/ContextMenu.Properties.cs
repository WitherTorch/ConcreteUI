using System;

namespace ShioUI.Controls;

partial class ContextMenu
{
    public event EventHandler? ItemClicked;

    public ContextMenuItem[] MenuItems { get; }
}
