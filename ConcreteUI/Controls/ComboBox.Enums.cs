using System;

namespace ConcreteUI.Controls
{
    partial class ComboBox
    {
        [Flags]
        private enum RenderObjectUpdateFlags
        {
            None = 0,
            Layout = 0b01,
            FormatAndLayout = 0b11
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
}
