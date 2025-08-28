using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Native;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Window2
{
    public partial class NativeWindow : CriticalFinalizerObject, IHwndOwner
    {
        private static readonly ThreadLocal<int> _threadIdLocal = new ThreadLocal<int>(Kernel32.GetCurrentThreadId, trackAllValues: false);

        private readonly ConcurrentBag<InvokeClosure> _invokeClosureBag = new ConcurrentBag<InvokeClosure>();
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
        private int  _invokeBarrier;
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
                    IntPtr handle = handleLazy.Value;
                    if (!WindowClassImpl.Instance.TryRegisterWindow(this))
                        DebugHelper.Throw();
                    OnHandleCreated(handle);
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

        public object? Invoke(Delegate @delegate, params object?[]? args)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The window is destroyed!");

            if (IsWindowThreadCore(handle))
                return @delegate.DynamicInvoke(this, args);
            return InvokeTaskCoreAsync(handle, @delegate, args, CancellationToken.None).Result;
        }

        public void InvokeAsync(Delegate @delegate, params object?[]? args)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The window is destroyed!");

            InvokeCoreAsync(handle, @delegate, args, CancellationToken.None);
        }

        public void InvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The window is destroyed!");

            InvokeCoreAsync(handle, @delegate, args, cancellationToken);
        }

        public Task<object?> InvokeTaskAsync(Delegate @delegate, params object?[]? args)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The window is destroyed!");

            return InvokeTaskCoreAsync(handle, @delegate, args, CancellationToken.None);
        }

        public Task<object?> InvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("The window is destroyed!");

            return InvokeTaskCoreAsync(handle, @delegate, args, cancellationToken);
        }

        private void InvokeCoreAsync(IntPtr handle, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            _invokeClosureBag.Add(new InvokeClosure(@delegate, args, null, cancellationToken));
            PostInvokeMessage(handle);
        }

        private async Task<object?> InvokeTaskCoreAsync(IntPtr handle, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _invokeClosureBag.Add(new InvokeClosure(@delegate, args, completionSource, cancellationToken));
            PostInvokeMessage(handle);
            return await completionSource.Task;
        }

        private void PostInvokeMessage(IntPtr handle)
        {
            if (MathHelper.ToBoolean(InterlockedHelper.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt), true))
                return;
            User32.PostMessageW(handle, CustomWindowMessages.ConcreteWindowInvoke, 0, 0);
            InterlockedHelper.Exchange(ref _invokeBarrier, Booleans.FalseInt);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool IsWindowThreadCore(IntPtr handle)
            => _threadIdLocal.Value == User32.GetWindowThreadProcessId(handle, null);
    }
}
