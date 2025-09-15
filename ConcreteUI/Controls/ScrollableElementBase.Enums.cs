using System;

namespace ConcreteUI.Controls
{
    partial class ScrollableElementBase
    {
        [Flags]
        protected enum ScrollableElementUpdateFlags : ulong
        {
            None = 0b000,
            Content = 0b001,
            ScrollBar = 0b010,
            All = Content | ScrollBar,
            RecalcScrollBar = 0b100,
            RecalcLayout = 0b1000 | RecalcScrollBar,

            _TriggerRenderEventStart = 0b10000,
            TriggerViewportPointChanged = _TriggerRenderEventStart,

            _NormalFlagAllTrue = _TriggerRenderEventStart - 1
        }

        private enum Brush
        {
            ScrollBarBackBrush,
            ScrollBarForeBrush,
            ScrollBarForeBrushHovered,
            ScrollBarForeBrushPressed,
            _Last
        }
    }
}
