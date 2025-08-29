using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Native;

using InlineMethod;

namespace ConcreteUI.Utils
{
    public static class MessageBox
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogCommandId Show(string text, string caption)
            => Show(IntPtr.Zero, text, caption, MessageBoxFlags.Ok);

        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogCommandId Show(string text, string caption, MessageBoxFlags flags)
            => Show(IntPtr.Zero, text, caption, flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe DialogCommandId Show(IntPtr hWnd, string text, string caption, MessageBoxFlags flags)
        {
            fixed (char* ptr = text, ptr2 = caption)
                return User32.MessageBoxW(hWnd, ptr, ptr2, flags);
        }
    }
}
