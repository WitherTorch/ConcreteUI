﻿using System;

namespace ConcreteUI.Controls
{
    partial class ScrollableElementBase
    {
        [Flags]
        protected enum UpdateFlags : long
        {
            None = 0b000,
            Content = 0b001,
            ScrollBar = 0b010,
            All = Content | ScrollBar,
            RecalcScrollBar = 0b100
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
