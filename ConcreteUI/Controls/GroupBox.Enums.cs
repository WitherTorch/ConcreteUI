using System;

namespace ConcreteUI.Controls
{
    partial class GroupBox
    {
        [Flags]
        private enum RenderObjectUpdateFlags
        {
            None = 0,
            Title = 0b01,
            Text = 0b10,
            All = Title | Text,
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
