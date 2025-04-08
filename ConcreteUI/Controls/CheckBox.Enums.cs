using System;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private enum RedrawType : long
        {
            NoRedraw,
            RedrawCheckBox,
            RedrawAllContent
        }

        [Flags]
        private enum RenderObjectUpdateFlags : long
        {
            None = 0,
            Layout = 0b01,
            Format = 0b11,
            FlagsAllTrue = -1L
        }

        private enum Brush
        {
            BorderBrush,
            BorderHoveredBrush,
            BorderPressedBrush,
            BorderCheckedBrush,
            BorderHoveredCheckedBrush,
            BorderPressedCheckedBrush,
            MarkBrush,
            TextBrush,
            _Last
        }
    }
}
