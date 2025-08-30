using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Internals;
using ConcreteUI.Native;
using ConcreteUI.Utils;

using InlineMethod;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Window
{
    public abstract partial class NativeWindow : CriticalFinalizerObject, IHwndOwner
    {
        private readonly WeakReference<IHwndOwner>? _parentReference;
        private readonly Lazy<IntPtr> _handleLazy;

        private Win32ImageHandle _cursor;
        private Icon? _cachedIcon;
        private string? _cachedText;
        private Rectangle _cachedBounds;
        /* Window state
         * bit[0] = already shown or not
         * bit[1] = focused or not
         * (all bit set) = handle is destroyed
         */
        private nuint _windowState;
        private uint _closeReason, _dialogResult;
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
        }

        public void Show()
        {
            if ((InterlockedHelper.Or(ref _windowState, 0b01) & 0b01) == 0b01)
                return;
            WindowMessageLoop.Invoke(() =>
            {
                Lazy<IntPtr> handleLazy = _handleLazy;
                if (!handleLazy.IsValueCreated)
                {
                    IntPtr handle = handleLazy.Value;
                    if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                        DebugHelper.Throw();
                    OnHandleCreated(handle);
                }
                ShowCore();
            });
        }

        public async Task ShowAsync()
        {
            if ((InterlockedHelper.Or(ref _windowState, 0b01) & 0b01) == 0b01)
                return;
            await WindowMessageLoop.InvokeTaskAsync(() =>
            {
                Lazy<IntPtr> handleLazy = _handleLazy;
                if (!handleLazy.IsValueCreated)
                {
                    IntPtr handle = handleLazy.Value;
                    if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                        DebugHelper.Throw();
                    OnHandleCreated(handle);
                }
                ShowCore();
            });
        }

        public DialogResult ShowDialog()
        {
            if ((InterlockedHelper.Or(ref _windowState, 0b01) & 0b01) == 0b01)
                return DialogResult.Invalid;
            WindowMessageLoop.Invoke(ShowDialogCore);
            return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
        }

        public async Task<DialogResult> ShowDialogAsync()
        {
            if ((InterlockedHelper.Or(ref _windowState, 0b01) & 0b01) == 0b01)
                return DialogResult.Invalid;
            await WindowMessageLoop.InvokeTaskAsync(ShowDialogCore);
            return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
        }

        private void ShowDialogCore()
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

            ShowCore();
            IntPtr parent = User32.GetWindow(handle, GetWindowCommand.Owner);
            if (parent == IntPtr.Zero)
            {
                const int GWLP_HWNDPARENT = -8;

                parent = User32.GetActiveWindow();
                User32.SetWindowLongPtrW(handle, GWLP_HWNDPARENT, parent);
            }
            using CancellationTokenSource destroyTokenSource = new CancellationTokenSource();
            void OnDestroyed(object? sender, EventArgs args)
            {
                Destroyed -= OnDestroyed;
                destroyTokenSource.Cancel();
            }
            User32.EnableWindow(parent, false);
            WindowMessageLoop.StartMiniLoop(destroyTokenSource.Token);
            User32.EnableWindow(parent, true);
            if (User32.IsWindowVisible(parent))
                User32.SetActiveWindow(parent);
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

        protected virtual void ShowCore()
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
            {
                Thread.MemoryBarrier();
                handle = Handle;
                if (handle == IntPtr.Zero)
                    return;
            }
            ShowCore(handle, WindowState.Normal);
        }
    }
}
