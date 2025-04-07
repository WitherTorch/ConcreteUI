using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class ComboBox
    {
        public event EventHandler ItemClicked;
        public event EventHandler<DropdownListEventArgs> DropdownListOpened;

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                    _dropdownList.Close();
                if (_state != ButtonTriState.None)
                    _state = ButtonTriState.None;
                Update();
            }
        }

        public ComboBoxDropdownList DropdownList
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dropdownList;
        }

        public int SelectedIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _selectedIndex;
            set
            {
                int oldSelectedIndex = _selectedIndex;
                if (oldSelectedIndex == value)
                    return;
                if (value < 0)
                {
                    _selectedIndex = -1;
                    Text = string.Empty;
                    return;
                }
                IList<string> items = _items.GetUnderlyingList();
                value = MathHelper.Min(value, items.Count);
                if (oldSelectedIndex == value)
                    return;
                _selectedIndex = value;
                Text = items[value];
            }
        }

        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;
                _fontSize = value;
                Update(RenderObjectUpdateFlags.FormatAndLayout);
            }
        }

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
            set
            {
                if (ReferenceEquals(_text, value))
                    return;
                _text = value;
                Update(RenderObjectUpdateFlags.Layout);
            }
        }

        public IList<string> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _items;
        }

        public int DropdownListVisibleCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dropDownListVisibleCount;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _dropDownListVisibleCount = value;
        }
    }
}
