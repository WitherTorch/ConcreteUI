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

        public readonly Point Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location;
        }

        public readonly int X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data._location.X;
        }

        public readonly int Y
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
        public MouseNotifyEventArgs(Point point) : this(point, MouseButtons.None, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(Point point, MouseButtons buttons) : this(point, buttons, 0) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(Point point, short delta) : this(point, MouseButtons.None, delta) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MouseNotifyEventArgs(Point point, MouseButtons buttons, short delta) : this(new MouseEventData(point, buttons, delta)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MouseNotifyEventArgs(in MouseEventData data)
            => _data = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MouseNotifyEventArgs(in MouseInteractEventArgs args)
            => UnsafeHelper.As<MouseInteractEventArgs, MouseNotifyEventArgs>(ref UnsafeHelper.AsRefIn(in args));
    }
}
