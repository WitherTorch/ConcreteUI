using System;
using System.Windows.Forms;

using ConcreteUI.Native;

using InlineMethod;

using WitherTorch.Common;

namespace ConcreteUI.Input.NativeHelper
{
    /// <summary>
    /// Represent a Input Method Context (IMC)
    /// </summary>
    internal class InputMethodContext : IDisposable
    {
        private IntPtr himc;
        public bool Status
        {
            get => Imm32.ImmGetOpenStatus(himc); set => Imm32.ImmSetOpenStatus(himc, value);
        }

        private InputMethodContext(IntPtr himc)
        {
            this.himc = himc;
        }

        public static InputMethodContext Empty => new InputMethodContext(IntPtr.Zero);

        public static InputMethodContext Create()
        {
            return new InputMethodContext(Imm32.ImmCreateContext());
        }

        public static InputMethodContext FromHandle(IntPtr Handle)
        {
            return new HandledInputMethodContext(Handle);
        }

        public static void Associate(IntPtr Handle, InputMethodContext? context = null)
        {
            IntPtr oldHimc = Imm32.ImmAssociateContext(Handle, context ?? IntPtr.Zero);
            if (oldHimc != IntPtr.Zero && oldHimc != (context ?? IntPtr.Zero))
            {
                try
                {
                    Imm32.ImmReleaseContext(Handle, oldHimc);
                }
                catch (Exception)
                {
                    try
                    {
                        Imm32.ImmDestroyContext(oldHimc);
                    }
                    catch (Exception)
                    {
                        // Maybe memory leak...
                    }
                }
            }
        }

        public static void Associate(IntPtr Handle, out InputMethodContext oldContext, InputMethodContext? newContext = null)
        {
            IntPtr oldHimc = Imm32.ImmAssociateContext(Handle, newContext ?? IntPtr.Zero);
            if (oldHimc == IntPtr.Zero) oldContext = Empty;
            else oldContext = oldHimc;
        }

        public static implicit operator IntPtr(InputMethodContext source) => source.himc;

        public static implicit operator InputMethodContext(IntPtr source) => new InputMethodContext(source);

        public static bool operator ==(InputMethodContext? a, InputMethodContext? b)
        {
            if (a is null)
                return b is null;
            if (b is null)
                return false;
            return a.himc == b.himc;
        }

        public static bool operator !=(InputMethodContext? a, InputMethodContext? b) => !(a == b);

        public static bool operator ==(InputMethodContext? a, IntPtr b) => a is not null && a.himc == b;
        public static bool operator !=(InputMethodContext? a, IntPtr b) => a is null || a.himc != b;
        public static bool operator ==(IntPtr a, InputMethodContext? b) => b is not null && a == b.himc;
        public static bool operator !=(IntPtr a, InputMethodContext? b) => b is null || a != b.himc;

        public override bool Equals(object? obj)
        {
            if (obj is InputMethodContext context)
            {
                return himc.Equals(context.himc);
            }
            else if (obj is IntPtr himc)
            {
                return this.himc.Equals(himc);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return himc.GetHashCode();
        }

        public override string ToString()
        {
            return himc.ToString();
        }

        public bool IsEmpty => ReferenceEquals(this, Empty) || himc == IntPtr.Zero;

        [Inline(InlineBehavior.Remove)]
        public unsafe int GetCursorPosition()
        {
            return GetIMEValue(IMECompositionFlags.CursorPosition);
        }

        [Inline(InlineBehavior.Remove)]
        public unsafe string GetCompositionString()
        {
            return GetIMEString(IMECompositionFlags.CompositionString);
        }

        [Inline(InlineBehavior.Remove)]
        public unsafe string GetResultString()
        {
            return GetIMEString(IMECompositionFlags.ResultString);
        }

        public unsafe string GetIMEString(IMECompositionFlags flags)
        {
            IntPtr himc = this.himc;
            long longSize = Imm32.ImmGetCompositionStringW(himc, flags, null, 0);
            if (longSize < 0 || longSize >= int.MaxValue)
                return string.Empty;
            int size = unchecked((int)longSize);
            if (size > Limits.MaxStackallocBytes)
            {
                fixed (byte* bufferPointer = new byte[size])
                {
                    longSize = Imm32.ImmGetCompositionStringW(himc, flags, bufferPointer, size);
                    if (longSize < 0 || longSize >= int.MaxValue)
                        return string.Empty;
                    return new string((char*)bufferPointer, 0, size / sizeof(char));
                }
            }
            else
            {
                byte* bufferPointer = stackalloc byte[size];
                longSize = Imm32.ImmGetCompositionStringW(himc, flags, bufferPointer, size);
                if (longSize < 0 || longSize >= int.MaxValue)
                    return string.Empty;
                return new string((char*)bufferPointer, 0, size / sizeof(char));
            }
        }

        public unsafe int GetIMEValue(IMECompositionFlags flags)
        {
            long value = Imm32.ImmGetCompositionStringW(himc, flags, null, 0);
            if (value < 0 || value >= int.MaxValue)
                return 0;
            return unchecked((int)value);
        }

        public unsafe void SetCandidateWindow(CandidateForm* pForm)
        {
            Imm32.ImmSetCandidateWindow(himc, pForm);
        }

        public unsafe void SetCompositionWindow(CompositionForm* pForm)
        {
            Imm32.ImmSetCompositionWindow(himc, pForm);
        }

        public Keys GetRealKeyCode()
        {
            IntPtr handle = himc;
            if (handle == IntPtr.Zero)
                return Keys.None;
            return (Keys)Imm32.ImmGetVirtualKey(handle) & Keys.KeyCode;
        }

        protected virtual void DisposeHIMC()
        {
            Imm32.ImmDestroyContext(himc);
        }

        private class HandledInputMethodContext : InputMethodContext
        {
            private IntPtr hwnd;
            internal HandledInputMethodContext(IntPtr hwnd) : base(Imm32.ImmGetContext(hwnd))
            {
                this.hwnd = hwnd;
            }

            protected override void DisposeHIMC()
            {
                Imm32.ImmReleaseContext(hwnd, himc);
            }
        }

        #region Disposing
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                if (himc != IntPtr.Zero)
                {
                    try
                    {
                        DisposeHIMC();
                    }
                    catch (Exception)
                    {
                    }
                }
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        ~InputMethodContext()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
