using System;
using System.Runtime.CompilerServices;

namespace ConcreteUI.Controls
{
    partial class ComboBoxDropdownList
    {
        public event EventHandler? ItemClicked;

        public new ComboBox Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _parent;
        }

        public int SelectedIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _selectedIndex;
        }
    }
}
