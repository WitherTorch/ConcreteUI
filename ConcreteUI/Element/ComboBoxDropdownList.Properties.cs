using System;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Element;

partial class ComboBoxDropdownList
{
    public event EventHandler<int>? ItemClicked;

    public new ComboBox Parent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _owner;
    }

    public int SelectedIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _selectedIndex;
    }
}
