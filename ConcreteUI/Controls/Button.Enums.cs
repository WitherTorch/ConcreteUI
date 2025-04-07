using System;

namespace ConcreteUI.Controls
{
    partial class Button
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
