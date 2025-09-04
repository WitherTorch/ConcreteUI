using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Controls
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct KeyInteractEventArgs : IInteractEventArgs
    {
        private readonly VirtualKey _key;
        private readonly ushort _repeatCount;

        private bool _handled;

        public readonly VirtualKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        public readonly ushort RepeatCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _repeatCount;
        }

        public readonly bool Handled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyInteractEventArgs(VirtualKey key) : this(key, 0) { }

        public KeyInteractEventArgs(VirtualKey key, ushort repeatCount)
        {
            _key = key;
            _repeatCount = repeatCount;
            _handled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Handle() => _handled = true;
    }
}
