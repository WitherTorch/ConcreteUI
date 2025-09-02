using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Internals
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    internal readonly struct MouseEventData
    {
        [FieldOffset(0)]
        public readonly PointF _location;
        [FieldOffset(0)]
        public readonly float _x;
        [FieldOffset(sizeof(float))]
        public readonly float _y;
        [FieldOffset(sizeof(float) * 2)]
        public readonly MouseButtons _buttons;
        [FieldOffset(sizeof(float) * 2 + sizeof(MouseButtons))]
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
