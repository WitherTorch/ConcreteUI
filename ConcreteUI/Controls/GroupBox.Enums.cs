using System;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        private enum RedrawType : long
        {
            NoRedraw,
            RedrawText,
            RedrawAllContent
        }

        [Flags]
        private enum RenderObjectUpdateFlags : long
        {
            None = 0,
            Title = 0b001,
            Text = 0b010,
            Format = 0b111,
            FlagsAllTrue = -1L
        }

        private enum Brush
        {
            BackBrush,
            BorderBrush,
            TextBrush,
            _Last
        }
    }
}
