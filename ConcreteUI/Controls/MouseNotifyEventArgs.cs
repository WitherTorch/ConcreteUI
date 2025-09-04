using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct MouseNotifyEventArgs
    {
        private readonly MouseEventData _data;

        public readonly PointF Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location;
        }

        public readonly float X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location.X;
        }

        public readonly float Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location.Y;
        }

        public readonly MouseButtons Buttons
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._buttons;
        }

        public readonly short Delta
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(PointF point) : this(point, MouseButtons.None, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(PointF point, MouseButtons buttons) : this(point, buttons, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(PointF point, short delta) : this(point, MouseButtons.None, delta) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(PointF point, MouseButtons buttons, short delta) : this(new MouseEventData(point, buttons, delta)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MouseNotifyEventArgs(in MouseEventData data)
            => _data = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MouseNotifyEventArgs(in MouseInteractEventArgs args)
            => UnsafeHelper.As<MouseInteractEventArgs, MouseNotifyEventArgs>(ref UnsafeHelper.AsRefIn(in args));
    }
}
