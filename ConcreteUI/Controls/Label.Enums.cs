using System;

namespace ConcreteUI.Controls
{
    partial class Label
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
            ForeBrush,
            _Last
        }
    }
}
