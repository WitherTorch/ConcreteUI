using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Internals
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal readonly struct MouseEventData
    {
        public readonly PointF _location;
        public readonly MouseButtons _buttons;
        public readonly short _delta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseEventData(PointF point) : this(point, MouseButtons.None, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseEventData(PointF point, MouseButtons buttons) : this(point, buttons, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseEventData(PointF point, short delta) : this(point, MouseButtons.None, delta) { }

        public MouseEventData(PointF point, MouseButtons buttons, short delta)
        {
            _location = point;
            _buttons = buttons;
            _delta = delta;
        }
    }
}
