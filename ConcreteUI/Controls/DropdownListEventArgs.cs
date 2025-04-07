using System;

namespace ConcreteUI.Controls
{
    public sealed class DropdownListEventArgs : EventArgs
    {
        public ComboBoxDropdownList DropdownList { get; }

        public DropdownListEventArgs(ComboBoxDropdownList dropdownList)
        {
            DropdownList = dropdownList;
        }
    }
}
