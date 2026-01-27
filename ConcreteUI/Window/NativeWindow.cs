using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Window
{
    public abstract partial class NativeWindow : CriticalFinalizerObject, IHwndOwner
    {
        private readonly WeakReference<IHwndOwner>? _parentReference;
        private readonly Lazy<IntPtr> _handleLazy;

        private CancellationTokenSource? _dialogTokenSource;
        private Win32ImageHandle _cursor;
        private Icon? _cachedIcon;
        private string? _cachedText;
        private Rectangle _cachedBounds;
        private IntPtr _dialogParent;
        /* Window flags
         * bit[0] = show() called or not
         * bit[1] = already shown or not
         * bit[2] = focused or not
         * (all bit set) = handle is destroyed
         */
        private nuint _windowFlags;
        private uint _windowState, _closeReason, _dialogResult;
        private bool _disposed;

        public NativeWindow(IHwndOwner? parent = null)
        {
            _parentReference = parent is null ? null : new WeakReference<IHwndOwner>(parent);
            _handleLazy = new Lazy<IntPtr>(() =>
            {
                IntPtr parentHandle = IntPtr.Zero;
                if (_parentReference?.TryGetTarget(out IHwndOwner? parent) == true)
                    parentHandle = parent.Handle;
                return CreateWindowHandle(parentHandle);
            }, LazyThreadSafetyMode.None);
            _cursor = SystemCursors.Default;
            _dialogTokenSource = null;
        }

        public void Show()
        {
            if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
                return;
            WindowMessageLoop.Invoke(ShowCore);
        }

        public async Task ShowAsync()
        {
            if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
                return;
            await WindowMessageLoop.InvokeTaskAsync(ShowCore);
        }

        public DialogResult ShowDialog()
        {
            if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
                return DialogResult.Invalid;
            WindowMessageLoop.Invoke(ShowDialogCore);
            return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
        }

        public async Task<DialogResult> ShowDialogAsync()
        {
            if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
                return DialogResult.Invalid;
            await WindowMessageLoop.InvokeTaskAsync(ShowDialogCore);
            return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
        }

        private void ShowCore() => ShowCoreWithReturn();

        private IntPtr ShowCoreWithReturn()
        {
            Lazy<IntPtr> handleLazy = _handleLazy;
            IntPtr handle;
            if (!handleLazy.IsValueCreated)
            {
                handle = handleLazy.Value;
                if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                    DebugHelper.Throw();
                OnHandleCreated(handle);
            }
            else
                handle = handleLazy.Value;

            ShowWindow(handle, WindowState.Normal);
            return handle;
        }

        private void ShowDialogCore()
        {
            IntPtr parent = FindParentHandleForDialog(handle: ShowCoreWithReturn());
            User32.EnableWindow(parent, false);
            InterlockedHelper.Exchange(ref _dialogParent, parent);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            InterlockedHelper.Exchange(ref _dialogTokenSource, tokenSource);
            WindowMessageLoop.StartMiniLoop(tokenSource.Token);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void Close() => Close(CloseReason.Programmically);

        public void Close(CloseReason reason)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                return;
            InterlockedHelper.Exchange(ref _closeReason, (uint)reason);
            User32.PostMessageW(handle, WindowMessage.Close, 0, 0);
        }

        private static IntPtr FindParentHandleForDialog(IntPtr handle)
        {
            IntPtr parent = User32.GetWindow(handle, GetWindowCommand.Owner);
            if (parent != IntPtr.Zero)
                return parent;

            const int GWLP_HWNDPARENT = -8;

            parent = User32.GetActiveWindow();
            User32.SetWindowLongPtrW(handle, GWLP_HWNDPARENT, parent);
            return parent;
        }
    }
}
