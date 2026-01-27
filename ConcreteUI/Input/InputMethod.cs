using System;

using ConcreteUI.Input.NativeHelper;
using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using WitherTorch.Common.Structures;

namespace ConcreteUI.Input
{
    public sealed class InputMethod : IWindowMessageFilter, IDisposable
    {
        private readonly CoreWindow _owner;

        private bool _imeStatus;
        private ushort _langId;
        private IntPtr _windowHandle;
        private InputMethodContext? _context;
        private IIMEControl? _attachedControl;

        private bool _disposed;

        public InputMethod(CoreWindow window)
        {
            _owner = window;
            _windowHandle = window.Handle;
            _owner.AddMessageFilter(this);
            _imeStatus = true;
            _langId = User32Utils.GetCurrentInputLanguage();
        }

        public void Attach(IIMEControl control)
        {
            IIMEControl? oldControl = _attachedControl;
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
            SetCandidateWindow(context, control.GetInputArea());
        }

        public void Detach(IIMEControl control)
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

            IIMEControl? attachedControl = _attachedControl;
            switch (message)
            {
                case WindowMessage.InputLanguageChange:
                    _langId = User32Utils.GetCurrentInputLanguage();
                    break;
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
                    attachedControl.StartIMEComposition();
                    return true;
                case WindowMessage.ImeEndComposition:
                    attachedControl.EndIMEComposition();
                    return false;
                case WindowMessage.ImeComposition:
                    {
                        InputMethodContext? context = _context;
                        if (context is null)
                            break;

                        IMECompositionFlags flags = (IMECompositionFlags)lParam;
                        if ((flags & IMECompositionFlags.CompositionString) > 0)
                        {
                            int cursorPos;
                            if ((flags & IMECompositionFlags.CursorPosition) > 0)
                                cursorPos = context.GetCursorPosition();
                            else
                                cursorPos = -1;
                            attachedControl.OnIMEComposition(context.GetCompositionString(), flags, cursorPos);
                            return true;
                        }
                        if ((flags & IMECompositionFlags.ResultString) > 0)
                        {
                            attachedControl.OnIMECompositionResult(context.GetResultString(), flags);
                            return true;
                        }
                    }
                    return true;
            }
            return false;
        }

        private ushort GetPrimaryLangID()
        {
            return (ushort)(_langId & 0x3ff);
        }

        private const int LANG_CHINESE = 0x04;
        private const int LANG_KOREAN = 0x12;
        private const int LANG_JAPANESE = 0x11;

        private const int CFS_CANDIDATEPOS = 0x0040;
        private const int CFS_POINT = 0x0002;
        private const int CFS_EXCLUDE = 0x0080;

        private const int kCaretMargin = 1;

        // Copy from IMESharp
        internal unsafe void SetCandidateWindow(InputMethodContext context, in Rect caretRect)
        {
            int x = caretRect.Left;
            int y = caretRect.Top;

            ushort primaryLangID = GetPrimaryLangID();
            if (primaryLangID == LANG_CHINESE)
            {
                // Chinese IMEs ignore function calls to ::ImmSetCandidateWindow()
                // when a user disables TSF (Text Service Framework) and CUAS (Cicero
                // Unaware Application Support).
                // On the other hand, when a user enables TSF and CUAS, Chinese IMEs
                // ignore the position of the current system caret and uses the
                // parameters given to ::ImmSetCandidateWindow() with its 'dwStyle'
                // parameter CFS_CANDIDATEPOS.
                // Therefore, we do not only call ::ImmSetCandidateWindow() but also
                // set the positions of the temporary system caret.
                var candidateForm = new CandidateForm()
                {
                    dwStyle = CFS_CANDIDATEPOS,
                    ptCurrentPos = new System.Drawing.Point(x, y)
                };
                context.SetCandidateWindow(&candidateForm);
            }

            if (primaryLangID == LANG_JAPANESE)
                User32.SetCaretPos(x, caretRect.Bottom);
            else
                User32.SetCaretPos(x, y);

            // Set composition window position also to ensure move the candidate window position.
            var compositionForm = new CompositionForm
            {
                dwStyle = CFS_POINT,
                ptCurrentPos = new System.Drawing.Point(x, y)
            };
            context.SetCompositionWindow(&compositionForm);

            if (primaryLangID == LANG_KOREAN)
            {
                // Chinese IMEs and Japanese IMEs require the upper-left corner of
                // the caret to move the position of their candidate windows.
                // On the other hand, Korean IMEs require the lower-left corner of the
                // caret to move their candidate windows.
                y += kCaretMargin;
            }

            // Need to return here since some Chinese IMEs would stuck if set
            // candidate window position with CFS_EXCLUDE style.
            if (primaryLangID == LANG_CHINESE) return;

            // Japanese IMEs and Korean IMEs also use the rectangle given to
            // ::ImmSetCandidateWindow() with its 'dwStyle' parameter CFS_EXCLUDE
            // to move their candidate windows when a user disables TSF and CUAS.
            // Therefore, we also set this parameter here.
            var excludeRectangle = new CandidateForm();
            compositionForm.dwStyle = CFS_EXCLUDE;
            compositionForm.ptCurrentPos.X = x;
            compositionForm.ptCurrentPos.Y = y;
            compositionForm.rcArea.Left = x;
            compositionForm.rcArea.Top = y;
            compositionForm.rcArea.Right = caretRect.Right;
            compositionForm.rcArea.Bottom = caretRect.Bottom;
            context.SetCandidateWindow(&excludeRectangle);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }
                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                _context?.Dispose();
                // TODO: 將大型欄位設為 Null
                _disposed = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        ~InputMethod()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            try
            {
                _owner.RemoveMessageFilter(this);
            }
            catch (Exception)
            {
            }
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
