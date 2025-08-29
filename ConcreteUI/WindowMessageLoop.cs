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
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI
{
    public static partial class WindowMessageLoop
    {
        private static readonly ThreadLocal<uint> _threadIdLocal = new ThreadLocal<uint>(Kernel32.GetCurrentThreadId, trackAllValues: false);
        private static readonly ConcurrentBag<InvokeClosure> _invokeClosureBag = new ConcurrentBag<InvokeClosure>();
        private static readonly UnwrappableList<IWindowMessageFilter> _filterList = new UnwrappableList<IWindowMessageFilter>();

        private static NativeWindow? _mainWindow;
        private static uint _invokeBarrier, _threadIdForMessageLoop;

        public static event MessageLoopExceptionEventHandler? ExceptionCaught;

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

        public static void ChangeMainWindow(NativeWindow mainWindow)
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                throw new InvalidOperationException("The message loop is not exists!");
            ChangeMainWindowCore(mainWindow);
        }

        private static void ChangeMainWindowCore(NativeWindow? mainWindow)
        {
            if (mainWindow is not null)
                mainWindow.Destroyed += OnWindowDestroyed;
            NativeWindow? oldWindow = InterlockedHelper.Exchange(ref _mainWindow, mainWindow);
            if (oldWindow is not null)
                oldWindow.Destroyed -= OnWindowDestroyed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Start(NativeWindow mainWindow, bool disposeAfterDestroyed = true, bool catchAllExceptionIntoEventHandler = false)
        {
            uint currentThreadId = _threadIdLocal.Value;
            if (InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, currentThreadId, 0) != 0)
                throw new InvalidOperationException("Message loop is already exists!");

            mainWindow.Show();
            ChangeMainWindowCore(mainWindow);
            int result = catchAllExceptionIntoEventHandler ? DoMessageLoop_CatchAllException() : DoMessageLoop();
            InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);
            ChangeMainWindowCore(null);
            if (disposeAfterDestroyed)
                mainWindow.Dispose();
            return result;
        }

        private static unsafe int DoMessageLoop()
            => DoMessageLoop_Model(catchException: false);

        private static unsafe int DoMessageLoop_CatchAllException()
            => DoMessageLoop_Model(catchException: true);

        [Inline(InlineBehavior.Remove)]
        private static unsafe int DoMessageLoop_Model([InlineParameter] bool catchException)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            PumpingMessage msg;
            SysBool success;

            while (success = User32.GetMessageW(&msg, IntPtr.Zero, 0u, 0u))
            {
                if (success.IsFailed)
                {
                    if (catchException)
                    {
                        MessageLoopExceptionEventHandler? eventHandler = ExceptionCaught;
                        if (eventHandler is not null)
                        {
                            Exception? exception = Marshal.GetExceptionForHR(User32.GetLastError());
                            if (exception is not null)
                                eventHandler.Invoke(null, new MessageLoopExceptionEventArgs(exception));
                        }
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(User32.GetLastError());
                    }
                    return -1;
                }
                if (msg.hwnd == IntPtr.Zero && (uint)msg.message == CustomWindowMessages.ConcreteWindowInvoke)
                {
                    ConcurrentBag<InvokeClosure> invokeClosureBag = _invokeClosureBag;
                    while (invokeClosureBag.TryTake(out InvokeClosure? closure))
                    {
                        if (catchException)
                        {
                            try
                            {
                                closure.Invoke();
                            }
                            catch (Exception ex)
                            {
                                ExceptionCaught?.Invoke(null, new MessageLoopExceptionEventArgs(ex));
                            }
                        }
                        else
                        {
                            closure.Invoke();
                        }
                    }
                    continue;
                }
                lock (filterList)
                {
                    int count = filterList.Count;
                    if (count > 0)
                    {
                        ref IWindowMessageFilter filterRef = ref filterList.Unwrap()[0];
                        for (nuint i = 0, limit = unchecked((nuint)count); i < limit; i++)
                        {
                            IWindowMessageFilter filter = UnsafeHelper.AddByteOffset(ref filterRef, i * UnsafeHelper.SizeOf<IWindowMessageFilter>());
                            if (catchException)
                            {
                                try
                                {
                                    if (filter.TryProcessWindowMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, out _))
                                        continue;
                                }
                                catch (Exception ex)
                                {
                                    ExceptionCaught?.Invoke(null, new MessageLoopExceptionEventArgs(ex));
                                }
                            }
                            else
                            {
                                if (filter.TryProcessWindowMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, out _))
                                    continue;
                            }
                        }
                    }
                }
                User32.TranslateMessage(&msg);
                User32.DispatchMessageW(&msg);
            }

            return unchecked((int)msg.wParam);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Stop(int exitCode = 0)
            => User32.PostQuitMessage(exitCode);

        private static void OnWindowDestroyed(object? sender, EventArgs e)
            => Stop();

        public static void AddMessageFilter(IWindowMessageFilter messageFilter)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            lock (filterList)
                filterList.Add(messageFilter);
        }

        public static void RemoveMessageFilter(IWindowMessageFilter messageFilter)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            lock (filterList)
                filterList.Remove(messageFilter);
        }

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
        public static void InvokeAsync(Delegate @delegate) => InvokeAsync(@delegate, null);
       
        [Inline(InlineBehavior.Keep, export: true)]
        public static void InvokeAsync(Delegate @delegate, CancellationToken cancellationToken) => InvokeAsync(@delegate, null, cancellationToken);

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
