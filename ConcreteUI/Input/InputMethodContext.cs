using System;
using System.Runtime.ConstrainedExecution;

using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Input
{
    /// <summary>
    /// Represent a Input Method Context (IMC)
    /// </summary>
    public sealed class InputMethodContext : CriticalFinalizerObject, ICheckableDisposable
    {
        private readonly IntPtr _hwnd, _himc;

        private bool _disposed;

        public bool IsDisposed => _disposed;

        public bool Status
        {
            get => Imm32.ImmGetOpenStatus(_himc);
            set => Imm32.ImmSetOpenStatus(_himc, value);
        }

        private InputMethodContext()
        {
            _hwnd = IntPtr.Zero;
            _himc = IntPtr.Zero;
            Dispose();
        }

        private InputMethodContext(IntPtr hwnd, IntPtr himc)
        {
            _hwnd = hwnd;
            _himc = himc;
            if (himc == IntPtr.Zero)
                Dispose();
        }

        public static InputMethodContext Empty => new InputMethodContext();

        public static InputMethodContext Create() => new InputMethodContext(IntPtr.Zero, Imm32.ImmCreateContext());

        public static InputMethodContext FromHandle(IntPtr hwnd) => new InputMethodContext(hwnd, Imm32.ImmGetContext(hwnd));

        public static void Associate(IntPtr Handle, InputMethodContext? context = null)
        {
            IntPtr oldHimc = Imm32.ImmAssociateContext(Handle, context?._himc ?? IntPtr.Zero);
            if (oldHimc != IntPtr.Zero && oldHimc != (context?._himc ?? IntPtr.Zero))
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

        public static void Associate(IntPtr hwnd, out InputMethodContext oldContext, InputMethodContext? newContext = null)
        {
            IntPtr oldHimc = Imm32.ImmAssociateContext(hwnd, newContext?._himc ?? IntPtr.Zero);
            if (oldHimc == IntPtr.Zero)
                oldContext = Empty;
            else
                oldContext = new InputMethodContext(hwnd, oldHimc);
        }

        public static explicit operator IntPtr(InputMethodContext source) => source._himc;

        public static explicit operator InputMethodContext(IntPtr source) => new InputMethodContext(IntPtr.Zero, source);

        public static bool operator ==(InputMethodContext? a, InputMethodContext? b)
        {
            if (a is null)
                return b is null;
            if (b is null)
                return false;
            return a._himc == b._himc;
        }

        public static bool operator !=(InputMethodContext? a, InputMethodContext? b) => !(a == b);

        public static bool operator ==(InputMethodContext? a, IntPtr b) => a is not null && a._himc == b;
        public static bool operator !=(InputMethodContext? a, IntPtr b) => a is null || a._himc != b;
        public static bool operator ==(IntPtr a, InputMethodContext? b) => b is not null && a == b._himc;
        public static bool operator !=(IntPtr a, InputMethodContext? b) => b is null || a != b._himc;

        public override bool Equals(object? obj)
        {
            if (obj is InputMethodContext context)
            {
                return _himc.Equals(context._himc);
            }
            else if (obj is IntPtr himc)
            {
                return this._himc.Equals(himc);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _himc.GetHashCode();
        }

        public override string ToString()
        {
            return _himc.ToString();
        }

        public bool IsEmpty => _himc == IntPtr.Zero;

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
            IntPtr himc = this._himc;
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
            long value = Imm32.ImmGetCompositionStringW(_himc, flags, null, 0);
            if (value < 0 || value >= int.MaxValue)
                return 0;
            return unchecked((int)value);
        }

        public unsafe bool SetCandidateWindow(in IMECandidateForm form) => SetCandidateWindow(UnsafeHelper.AsPointerIn(in form));

        public unsafe bool SetCandidateWindow(IMECandidateForm* pForm) => Imm32.ImmSetCandidateWindow(_himc, pForm);

        public unsafe bool SetCompositionWindow(in IMECompositionForm form) => SetCompositionWindow(UnsafeHelper.AsPointerIn(in form));

        public unsafe bool SetCompositionWindow(IMECompositionForm* pForm) => Imm32.ImmSetCompositionWindow(_himc, pForm);

        public VirtualKey GetRealKeyCode()
        {
            IntPtr handle = _himc;
            if (handle == IntPtr.Zero)
                return VirtualKey.None;
            return (VirtualKey)Imm32.ImmGetVirtualKey(handle);
        }

        private void DisposeCore()
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            IntPtr himc = _himc;
            IntPtr hwnd = _hwnd;
            if (himc == IntPtr.Zero)
                return;
            if (hwnd == IntPtr.Zero)
                Imm32.ImmDestroyContext(himc);
            else
                Imm32.ImmReleaseContext(hwnd, himc);
        }

        ~InputMethodContext() => DisposeCore();

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
