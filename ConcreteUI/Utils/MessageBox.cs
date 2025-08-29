using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using ConcreteUI.Native;
using ConcreteUI.Window;

using InlineMethod;

namespace ConcreteUI.Utils
{
    public static class MessageBox
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogResult Show(string text, string caption)
            => Show(IntPtr.Zero, text, caption, MessageBoxFlags.Ok);

        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogResult Show(string text, string caption, MessageBoxFlags flags)
            => Show(IntPtr.Zero, text, caption, flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DialogResult Show(IntPtr hWnd, string text, string caption, MessageBoxFlags flags)
            => (DialogResult)WindowMessageLoop.Invoke(() => ShowDirectly(hWnd, text, caption, flags))!;

        [Inline(InlineBehavior.Keep, export: true)]
        public static Task<DialogResult> ShowAsync(string text, string caption)
            => ShowAsync(IntPtr.Zero, text, caption, MessageBoxFlags.Ok);

        [Inline(InlineBehavior.Keep, export: true)]
        public static Task<DialogResult> ShowAsync(string text, string caption, MessageBoxFlags flags)
            => ShowAsync(IntPtr.Zero, text, caption, flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<DialogResult> ShowAsync(IntPtr hWnd, string text, string caption, MessageBoxFlags flags)
            => (DialogResult)(await WindowMessageLoop.InvokeTaskAsync(() => ShowDirectly(hWnd, text, caption, flags)))!;

        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogResult ShowDirectly(string text, string caption)
            => ShowDirectly(IntPtr.Zero, text, caption, MessageBoxFlags.Ok);

        [Inline(InlineBehavior.Keep, export: true)]
        public static DialogResult ShowDirectly(string text, string caption, MessageBoxFlags flags)
            => ShowDirectly(IntPtr.Zero, text, caption, flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe DialogResult ShowDirectly(IntPtr hWnd, string text, string caption, MessageBoxFlags flags)
        {
            fixed (char* ptr = text, ptr2 = caption)
                return User32.MessageBoxW(hWnd, ptr, ptr2, flags);
        }
    }
}
