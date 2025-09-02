using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Internals;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MouseInteractEventArgs : IInteractEventArgs
    {
        private readonly MouseEventData _data;

        private bool _handled;

        public readonly PointF Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location;
        }

        public readonly float X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._x;
        }

        public readonly float Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._y;
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

        public readonly bool Handled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseInteractEventArgs(PointF point) : this(point, MouseButtons.None, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseInteractEventArgs(PointF point, MouseButtons buttons) : this(point, buttons, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseInteractEventArgs(PointF point, short delta) : this(point, MouseButtons.None, delta) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseInteractEventArgs(PointF point, MouseButtons buttons, short delta) : this(new MouseEventData(point, buttons, delta)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MouseInteractEventArgs(in MouseEventData data)
        {
            _data = data;
            _handled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MouseInteractEventArgs(in MouseNotifyEventArgs args)
            => new MouseInteractEventArgs(UnsafeHelper.As<MouseNotifyEventArgs, MouseEventData>(ref UnsafeHelper.AsRefIn(in args)));

        public void Handle() => _handled = true;
    }
}
