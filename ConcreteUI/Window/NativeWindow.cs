using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using ConcreteUI.Internals;
using ConcreteUI.Native;
using ConcreteUI.Utils;

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
        private uint _closeReason;
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
            Lazy<IntPtr> handleLazy = _handleLazy;
            lock (handleLazy)
            {
                if (!handleLazy.IsValueCreated)
                {
                    WindowMessageLoop.Invoke(() =>
                    {
                        IntPtr handle = _handleLazy.Value;
                        if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                            DebugHelper.Throw();
                        OnHandleCreated(handle);
                    });
                }
            }
            ShowCore();
        }

        public void Close(CloseReason reason = CloseReason.Programmically)
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
