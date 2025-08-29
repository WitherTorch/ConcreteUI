using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using ConcreteUI.Internals;
using ConcreteUI.Native;
using ConcreteUI.Window;

using InlineMethod;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI
{
    public static partial class WindowMessageLoop
    {
        private static readonly ConcurrentBag<InvokeClosure> _invokeClosureBag = new ConcurrentBag<InvokeClosure>();
        private static readonly ThreadLocal<uint> _threadIdLocal = new ThreadLocal<uint>(Kernel32.GetCurrentThreadId, trackAllValues: false);

        private static uint _invokeBarrier, _threadIdForMessageLoop;

        public static uint CurrentThreadId => _threadIdLocal.Value;

        public static bool IsMessageLoopThread
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
                return messageLoopThreadId != 0 && _threadIdLocal.Value == messageLoopThreadId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int Start(NativeWindow window, bool disposeAfterDestroyed = true)
        {
            uint currentThreadId = _threadIdLocal.Value;
            if (InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, currentThreadId, 0) != 0)
                throw new InvalidOperationException("Message loop is already exists!");

            window.Destroyed += OnWindowDestroyed;
            window.Show();
            SysBool success;
            PumpingMessage msg;

            while (success = User32.GetMessageW(&msg, IntPtr.Zero, 0u, 0u))
            {
                if (success.IsFailed)
                {
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                    return -1;
                }
                if (msg.hwnd == IntPtr.Zero && (uint)msg.message == CustomWindowMessages.ConcreteWindowInvoke)
                {
                    ConcurrentBag<InvokeClosure> invokeClosureBag = _invokeClosureBag;
                    while (invokeClosureBag.TryTake(out InvokeClosure? closure))
                        closure.Invoke();
                    continue;
                }
                User32.TranslateMessage(&msg);
                User32.DispatchMessageW(&msg);
            }
            InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);
            int result = unchecked((int)msg.wParam);
            if (disposeAfterDestroyed)
                window.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Stop(int exitCode = 0)
            => User32.PostQuitMessage(exitCode);

        private static void OnWindowDestroyed(object? sender, EventArgs e)
            => Stop();

        [Inline(InlineBehavior.Keep, export: true)]
        public static object? Invoke(Delegate @delegate) => Invoke(@delegate, null);

        public static object? Invoke(Delegate @delegate, params object?[]? args)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");

            if (_threadIdLocal.Value == messageLoopThreadId)
                return @delegate.DynamicInvoke(args);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None).Result;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static object? InvokeAsync(Delegate @delegate) => Invoke(@delegate, null);

        public static void InvokeAsync(Delegate @delegate, params object?[]? args)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");

            InvokeCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None);
        }

        public static void InvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");

            InvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static Task<object?> InvokeTaskAsync(Delegate @delegate) => InvokeTaskAsync(@delegate, null);

        [Inline(InlineBehavior.Keep, export: true)]
        public static Task<object?> InvokeTaskAsync(Delegate @delegate, CancellationToken cancellationToken) => InvokeTaskAsync(@delegate, null, cancellationToken);

        public static Task<object?> InvokeTaskAsync(Delegate @delegate, params object?[]? args)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");

            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None);
        }

        public static Task<object?> InvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");

            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
        }

        private static void InvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            _invokeClosureBag.Add(new InvokeClosure(@delegate, args, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static async Task<object?> InvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _invokeClosureBag.Add(new InvokeClosure(@delegate, args, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            return await completionSource.Task;
        }

        private static void PostInvokeMessage(uint threadId)
        {
            if (MathHelper.ToBoolean(InterlockedHelper.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt), true))
                return;
            User32.PostThreadMessageW(threadId, CustomWindowMessages.ConcreteWindowInvoke, 0, 0);
            InterlockedHelper.Exchange(ref _invokeBarrier, Booleans.FalseInt);
        }
    }
}
