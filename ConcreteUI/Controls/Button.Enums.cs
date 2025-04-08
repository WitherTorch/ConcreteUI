using System;

namespace ConcreteUI.Controls
{
    partial class Button
    {
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
            FaceBrush,
            FaceHoveredBrush,
            FacePressedBrush,
            TextBrush,
            TextDisabledBrush,
            _Last
        }
    }
}
