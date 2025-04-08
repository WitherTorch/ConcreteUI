using System;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        [Flags]
        private enum RenderObjectUpdateFlags : long
        {
            None = 0,
            Layout = 0b001,
            WatermarkLayout = 0b010,
            Format = 0b111,
            FlagsAllTrue = -1L
        }

        private enum Brush
        {
            BackBrush,
            BackDisabledBrush,
            BorderBrush,
            BorderFocusedBrush,
            ForeBrush,
            ForeInactiveBrush,
            SelectionBackBrush,
            SelectionForeBrush,
            _Last
        }
    }
}
