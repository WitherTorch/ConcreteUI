using System.Runtime.CompilerServices;

using ConcreteUI.Internals.Native;

using InlineMethod;

namespace ConcreteUI.Utils
{
    public static class Keys
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsControlPressed() => IsKeyPressed(VirtualKey.Control);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsLeftControlPressed() => IsKeyPressed(VirtualKey.LeftControl);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsRightControlPressed() => IsKeyPressed(VirtualKey.RightControl);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsShiftPressed() => IsKeyPressed(VirtualKey.Shift);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsLeftShiftPressed() => IsKeyPressed(VirtualKey.LeftShift);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsRightShiftPressed() => IsKeyPressed(VirtualKey.RightShift);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsAltPressed() => IsKeyPressed(VirtualKey.Alt);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsLeftAltPressed() => IsKeyPressed(VirtualKey.LeftAlt);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsRightAltPressed() => IsKeyPressed(VirtualKey.RightAlt);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsNumLockToggled() => IsKeyToggled(VirtualKey.NumLock);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsCapsLockToggled() => IsKeyToggled(VirtualKey.CapsLock);

        [Inline(InlineBehavior.Keep, export: true)]
        public static bool IsScrollLockToggled() => IsKeyToggled(VirtualKey.ScrollLock);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed(VirtualKey key) => User32.GetKeyState(key) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyToggled(VirtualKey key) => (User32.GetKeyState(key) & 0b01) == 0b01;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool isPressed, bool isToggled) GetKeyState(VirtualKey key)
        {
            short result = User32.GetKeyState(key);
            return (isPressed: result < 0,
                isToggled: (result & 0b01) == 0b01);
        }
    }
}
