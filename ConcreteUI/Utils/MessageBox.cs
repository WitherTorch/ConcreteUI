using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using ConcreteUI.Internals.Native;
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

    [Flags]
    public enum MessageBoxFlags : uint
    {
        Ok = 0x00000000U,
        OkCancel = 0x00000001U,
        AbortRetryIgnore = 0x00000002U,
        YesNoCancel = 0x00000003U,
        YesNo = 0x00000004U,
        RetryCancel = 0x00000005U,
        CancelRetryContinue = 0x00000006U,

        IconHand = 0x00000010U,
        IconQuestion = 0x00000020U,
        IconExclamation = 0x00000030U,
        IconAsterisk = 0x00000040U,

        UserIcon = 0x00000080U,
        IconWarning = IconExclamation,
        IconError = IconHand,

        IconInformation = IconAsterisk,
        IconStop = IconHand,

        DefaultButton1 = 0x00000000U,
        DefaultButton2 = 0x00000100U,
        DefaultButton3 = 0x00000200U,
        DefaultButton4 = 0x00000300U,
        ApplicationModel = 0x00000000U,
        SystemModel = 0x00001000U,
        TaskModel = 0x00002000U,
        HelpButton = 0x00004000U,

        NoFocus = 0x00008000U,
        SetForeground = 0x00010000U,
        DefaultDesktopOnly = 0x00020000U,

        TopMost = 0x00040000U,
        Right = 0x00080000U,
        RTLReading = 0x00100000U,

        ServiceNotification = 0x00200000U,
        ServiceNotificationNt3x = 0x00040000U
    }
}
