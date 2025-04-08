using System;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        [Flags]
        private enum RenderObjectUpdateFlags
        {
            None = 0,
            Layout = 0b001,
            WatermarkLayout = 0b010,
            All = Layout | WatermarkLayout
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
