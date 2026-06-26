using System;

namespace ShioUI.Controls;

public sealed class DropdownListEventArgs : EventArgs
{
    public ComboBoxDropdownList DropdownList { get; }

    public DropdownListEventArgs(ComboBoxDropdownList dropdownList)
    {
        DropdownList = dropdownList;
    }
}
