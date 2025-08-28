using System;

using ConcreteUI.Native;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Window2
{
    public static class Clipboard
    {
        public static unsafe string GetText()
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            IntPtr dataHandle = User32.GetClipboardData(ClipboardFormat.UnicodeText);
            if (dataHandle == IntPtr.Zero)
                return string.Empty;
            char* ptr = (char*)Kernel32.GlobalLock(dataHandle);
            try
            {
                return new string(ptr);
            }
            finally
            {
                Kernel32.GlobalUnlock(dataHandle);
            }
        }

        public static bool HasText()
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            return User32.IsClipboardFormatAvailable(ClipboardFormat.UnicodeText);
        }

        public static unsafe void SetText(string? text)
        {
            using ClipboardToken token = ClipboardToken.Acquire();
            User32.EmptyClipboard();
            if (text is null)
                return;
            int length = text.Length;
            if (length <= 0)
                return;
            nuint byteCount = unchecked((nuint)length + 1) * sizeof(char);
            IntPtr dataHandle = Kernel32.GlobalAlloc(GlobalAllocFlags.Movable, byteCount);
            char* ptr = (char*)Kernel32.GlobalLock(dataHandle);
            try
            {
                fixed (char* source = text)
                    UnsafeHelper.CopyBlock(ptr, source, byteCount);
            }
            finally
            {
                Kernel32.GlobalUnlock(dataHandle);
            }
            User32.SetClipboardData(ClipboardFormat.UnicodeText, dataHandle);
        }
    }
}
