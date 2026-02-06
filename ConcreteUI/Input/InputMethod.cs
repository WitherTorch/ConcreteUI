using System;

using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Input
{
    public sealed class InputMethod : IWindowMessageFilter, ICheckableDisposable
    {
        private readonly CoreWindow _owner;

        private bool _imeStatus;
        private IntPtr _windowHandle;
        private InputMethodContext? _context;
        private IInputMethodHandler? _attachedControl;

        private bool _disposed;

        public bool IsDisposed => _disposed;
        public InputMethodContext? Context => _context;

        public InputMethod(CoreWindow window)
        {
            _owner = window;
            _windowHandle = window.Handle;
            _owner.AddMessageFilter(this);
            _imeStatus = true;
        }

        public void Attach(IInputMethodHandler control)
        {
            IInputMethodHandler? oldControl = _attachedControl;
            if (oldControl == control)
                return;
            if (control is null)
            {
                DetachCore();
                return;
            }
            InputMethodContext? context = _context;
            if (oldControl is null)
                User32.CreateCaret(_windowHandle, IntPtr.Zero, 2, 10);
            if (context is null)
            {
                context = InputMethodContext.Create();
                InputMethodContext.Associate(_windowHandle, context);
                context.Status = _imeStatus;
                _context = context;
            }
            _attachedControl = control;
        }

        public void Detach(IInputMethodHandler control)
        {
            if (control is null || _attachedControl != control)
                return;
            InputMethodContext? context = _context;
            if (context != null)
            {
                _context = null;
                _imeStatus = context.Status;
                context.Dispose();
            }
            DetachCore();
        }

        private void DetachCore()
        {
            User32.DestroyCaret();
            _attachedControl = null;
        }

        public VirtualKey GetRealKeyCode()
        {
            return _context?.GetRealKeyCode() ?? VirtualKey.None;
        }

        public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            result = 0;

            IInputMethodHandler? attachedControl = _attachedControl;
            switch (message)
            {
                case WindowMessage.KillFocus:
                    if (_context != null)
                        _imeStatus = _context.Status;
                    break;
                case WindowMessage.ImeChar:
                    return true;
                default:
                    break;
            }
            if (attachedControl is null)
                return false;
            switch (message)
            {
                case WindowMessage.Activate:
                    {
                        if (wParam != 1 && wParam != 2)
                            break;
                        InputMethodContext? newContext = _context;
                        InputMethodContext.Associate(hwnd, out InputMethodContext oldContext, newContext);
                        if (oldContext != newContext)
                            oldContext?.Dispose();
                    }
                    break;
                case WindowMessage.ImeSetContext:
                    {
                        if (wParam != 1)
                            break;
                        InputMethodContext? newContext = _context;
                        if (newContext is null)
                            break;
                        InputMethodContext.Associate(hwnd, out InputMethodContext oldContext, newContext);
                        if (oldContext != newContext)
                            oldContext?.Dispose();
                        newContext.Status = _imeStatus;
                    }
                    break;
                case WindowMessage.ImeStartComposition:
                    {
                        InputMethodContext? context = _context;
                        if (context is not null && !context.IsEmpty)
                            attachedControl.StartIMEComposition(this, context);
                    }
                    return true;
                case WindowMessage.ImeEndComposition:
                    {
                        InputMethodContext? context = _context;
                        if (context is not null && !context.IsEmpty)
                            attachedControl.EndIMEComposition(this, context);
                    }
                    return false;
                case WindowMessage.ImeComposition:
                    {
                        InputMethodContext? context = _context;
                        if (context is null || context.IsEmpty)
                            break;

                        IMECompositionFlags flags = (IMECompositionFlags)lParam;
                        if ((flags & IMECompositionFlags.CompositionString) > 0)
                        {
                            int cursorPos;
                            if ((flags & IMECompositionFlags.CursorPosition) > 0)
                                cursorPos = context.GetCursorPosition();
                            else
                                cursorPos = -1;
                            attachedControl.OnIMEComposition(this, context, context.GetCompositionString(), flags, cursorPos);
                            return true;
                        }
                        if ((flags & IMECompositionFlags.ResultString) > 0)
                        {
                            attachedControl.OnIMECompositionResult(this, context, context.GetResultString(), flags);
                            return true;
                        }
                    }
                    return true;
            }
            return false;
        }

        private void DisposeCore()
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            _owner.RemoveMessageFilter(this);
            _context?.Dispose();
        }

        ~InputMethod() => DisposeCore();

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
