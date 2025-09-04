using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Controls
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CharacterInteractEventArgs : IInteractEventArgs
    {
        private readonly char _character;

        private bool _handled;

        public readonly char Character
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _character;
        }

        public readonly bool Handled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CharacterInteractEventArgs(char character)
        {
            _character = character;
            _handled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Handle() => _handled = true;
    }
}
