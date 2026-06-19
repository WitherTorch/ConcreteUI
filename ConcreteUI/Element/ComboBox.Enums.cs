using System;

namespace ConcreteUI.Element;

partial class ComboBox
{
    [Flags]
    private enum RenderObjectUpdateFlags
    {
        None = 0,
        Layout = 0b01,
        Format = 0b11
    }

    private enum Brush
    {
        BackBrush,
        BackDisabledBrush,
        BackHoveredBrush,
        BorderBrush,
        TextBrush,
        DropdownButtonBrush,
        DropdownButtonHoveredBrush,
        DropdownButtonPressedBrush,
        _Last
    }
}
