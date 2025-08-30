using System;
using System.Collections.Concurrent;
using System.Reflection;
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
        private static readonly UnwrappableList<IWindowMessageFilter> _filterList = new UnwrappableList<IWindowMessageFilter>();

        private static NativeWindow? _mainWindow;
        private static InvokeMessageFilter? _invokeMessageFilter;
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
            {
                mainWindow.Destroyed += OnWindowDestroyed;
                mainWindow.Show();
            }
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
            InvokeMessageFilter invokeMessageFilter = new InvokeMessageFilter(catchAllExceptionIntoEventHandler);
            invokeMessageFilter.ExceptionCaught += static (sender, args) => ExceptionCaught?.Invoke(sender, args);
            AddMessageFilter(invokeMessageFilter);
            InterlockedHelper.Exchange(ref _invokeMessageFilter, invokeMessageFilter)?.ProcessAllInvoke();

            ChangeMainWindowCore(mainWindow);
            int result = catchAllExceptionIntoEventHandler ? DoMessageLoop_CatchAllException() : DoMessageLoop();
            InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);

            invokeMessageFilter = InterlockedHelper.CompareExchange(ref _invokeMessageFilter, null, invokeMessageFilter);
            if (invokeMessageFilter is not null)
            {
                RemoveMessageFilter(invokeMessageFilter);
                invokeMessageFilter.ProcessAllInvoke();
            }
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
                if (TryFilterMessage(ref msg, catchException: false, out nint result))
                {
                    if (User32.InSendMessage())
                        User32.ReplyMessage(result);
                }
                else
                {
                    User32.TranslateMessage(&msg);
                    User32.DispatchMessageW(&msg);
                }
            }

            return unchecked((int)msg.wParam);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void StartMiniLoop(CancellationToken cancellationToken)
        {
            PumpingMessage msg;
            SysBool success;

            while (success = User32.GetMessageW(&msg, IntPtr.Zero, 0u, 0u))
            {
                if (success.IsFailed)
                {
                    Marshal.ThrowExceptionForHR(User32.GetLastError());
                    User32.PostQuitMessage(-1);
                    return;
                }
                if (TryFilterMessage(ref msg, catchException: false, out nint result))
                {
                    if (User32.InSendMessage())
                        User32.ReplyMessage(result);
                }
                else
                {
                    User32.TranslateMessage(&msg);
                    User32.DispatchMessageW(&msg);
                }
                if (cancellationToken.IsCancellationRequested)
                    return;
                continue;
            }
            User32.PostQuitMessage((int)msg.wParam);
        }

        [Inline(InlineBehavior.Remove)]
        private static bool TryFilterMessage(ref PumpingMessage msg, [InlineParameter] bool catchException, out nint result)
        {
            UnwrappableList<IWindowMessageFilter> filterList = _filterList;
            lock (filterList)
            {
                int count = filterList.Count;
                if (count <= 0)
                    goto Failed;

                IntPtr hwnd = msg.hwnd;
                WindowMessage message = msg.message;
                nint wParam = msg.wParam;
                nint lParam = msg.lParam;
                ref IWindowMessageFilter filterRef = ref filterList.Unwrap()[0];
                for (nuint i = 0, limit = unchecked((nuint)count); i < limit; i++)
                {
                    IWindowMessageFilter filter = UnsafeHelper.AddByteOffset(ref filterRef, i * UnsafeHelper.SizeOf<IWindowMessageFilter>());
                    if (catchException)
                    {
                        try
                        {
                            if (filter.TryProcessWindowMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, out result))
                                return true;
                        }
                        catch (Exception ex)
                        {
                            ExceptionCaught?.Invoke(null, new MessageLoopExceptionEventArgs(ex));
                        }
                    }
                    else
                    {
                        if (filter.TryProcessWindowMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, out result))
                            return true;
                    }
                }
            }

        Failed:
            result = 0;
            return false;
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

        public static object? Invoke<TDelegate>(TDelegate @delegate) where TDelegate : Delegate
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return null;
            if (_threadIdLocal.Value == messageLoopThreadId)
            {
                if (typeof(TDelegate) == typeof(Action))
                {
                    UnsafeHelper.As<Delegate, Action>(@delegate).Invoke();
                    return null;
                }
                return @delegate.DynamicInvoke(null);
            }
            if (typeof(TDelegate) == typeof(Action))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), CancellationToken.None).Result;
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None).Result;
        }

        public static object? Invoke<TDelegate, TArg>(TDelegate @delegate, TArg arg) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return null;

            if (_threadIdLocal.Value == messageLoopThreadId)
            {
                if (typeof(TDelegate) == typeof(Action<TArg>))
                {
                    UnsafeHelper.As<Delegate, Action<TArg>>(@delegate).Invoke(arg);
                    return null;
                }
                return @delegate.DynamicInvoke(arg);
            }
            if (typeof(TDelegate) == typeof(Action<TArg>))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action<TArg>>(@delegate), arg, CancellationToken.None).Result;
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg], CancellationToken.None).Result;
        }

        public static object? Invoke<TDelegate, TArg1, TArg2>(TDelegate @delegate, TArg1 arg1, TArg2 arg2) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return null;

            if (_threadIdLocal.Value == messageLoopThreadId)
            {
                if (typeof(TDelegate) == typeof(Action<TArg1, TArg2>))
                {
                    UnsafeHelper.As<Delegate, Action<TArg1, TArg2>>(@delegate).Invoke(arg1, arg2);
                    return null;
                }
                return @delegate.DynamicInvoke(arg1, arg2);
            }
            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2>))
                return InvokeTaskCoreAsync(messageLoopThreadId,
                     UnsafeHelper.As<Delegate, Action<TArg1, TArg2>>(@delegate), arg1, arg2, CancellationToken.None);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2], CancellationToken.None).Result;
        }

        public static object? Invoke<TDelegate, TArg1, TArg2, TArg3>(TDelegate @delegate, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return null;

            if (_threadIdLocal.Value == messageLoopThreadId)
            {
                if (typeof(TDelegate) == typeof(Action<TArg1, TArg2, TArg3>))
                {
                    UnsafeHelper.As<Delegate, Action<TArg1, TArg2, TArg3>>(@delegate).Invoke(arg1, arg2, arg3);
                    return null;
                }
                return @delegate.DynamicInvoke(arg1, arg2);
            }
            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2, TArg3>))
                return InvokeTaskCoreAsync(messageLoopThreadId,
                        UnsafeHelper.As<Delegate, Action<TArg1, TArg2, TArg3>>(@delegate), arg1, arg2, arg3, CancellationToken.None).Result;
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2, arg3], CancellationToken.None).Result;
        }

        public static object? Invoke<TDelegate>(TDelegate @delegate, params object?[]? args) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action) && args is not null && args.Length != 0)
                throw new TargetParameterCountException(nameof(args));

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return null;

            if (_threadIdLocal.Value == messageLoopThreadId)
            {
                if (typeof(TDelegate) == typeof(Action))
                {
                    UnsafeHelper.As<Delegate, Action>(@delegate).Invoke();
                    return null;
                }
                return @delegate.DynamicInvoke(args);
            }
            if (typeof(TDelegate) == typeof(Action))
            {
                InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), CancellationToken.None).Wait();
                return null;
            }
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None).Result;
        }

        public static void InvokeAsync<TDelegate>(TDelegate @delegate) where TDelegate : Delegate
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return;
            if (typeof(TDelegate) == typeof(Action))
            {
                InvokeCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), CancellationToken.None);
                return;
            }
            InvokeCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
        }

        public static void InvokeAsync<TDelegate, TArg>(TDelegate @delegate, TArg arg) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return;
            if (typeof(TDelegate) == typeof(Action<TArg>))
            {
                InvokeCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action<TArg>>(@delegate), arg, CancellationToken.None);
                return;
            }
            InvokeCoreAsync(messageLoopThreadId, @delegate, [arg], CancellationToken.None);
        }

        public static void InvokeAsync<TDelegate, TArg1, TArg2>(TDelegate @delegate, TArg1 arg1, TArg2 arg2) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return;
            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2>))
            {
                InvokeCoreAsync(messageLoopThreadId,
                    UnsafeHelper.As<Delegate, Action<TArg1, TArg2>>(@delegate), arg1, arg2, CancellationToken.None);
                return;
            }
            InvokeCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2], CancellationToken.None);
        }

        public static void InvokeAsync<TDelegate, TArg1, TArg2, TArg3>(TDelegate @delegate, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return;
            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2, TArg3>))
            {
                InvokeCoreAsync(messageLoopThreadId,
                    UnsafeHelper.As<Delegate, Action<TArg1, TArg2, TArg3>>(@delegate), arg1, arg2, arg3, CancellationToken.None);
                return;
            }
            InvokeCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2, arg3], CancellationToken.None);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static void InvokeAsync<TDelegate>(TDelegate @delegate, params object?[]? args) where TDelegate : Delegate
            => InvokeAsync(@delegate, args, CancellationToken.None);

        public static void InvokeAsync<TDelegate>(TDelegate @delegate, object?[]? args, CancellationToken cancellationToken) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action) && args is not null && args.Length != 0)
                throw new TargetParameterCountException(nameof(args));

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return;

            if (typeof(TDelegate) == typeof(Action))
            {
                InvokeCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), cancellationToken);
                return;
            }
            InvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
        }

        public static Task<object?> InvokeTaskAsync<TDelegate>(TDelegate @delegate) where TDelegate : Delegate
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return Task.FromResult<object?>(null);

            if (typeof(TDelegate) == typeof(Action))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), CancellationToken.None);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
        }

        public static Task<object?> InvokeTaskAsync<TDelegate, TArg>(TDelegate @delegate, TArg arg) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return Task.FromResult<object?>(null);

            if (typeof(TDelegate) == typeof(Action<TArg>))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action<TArg>>(@delegate), arg, CancellationToken.None);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg], CancellationToken.None);
        }

        public static Task<object?> InvokeTaskAsync<TDelegate, TArg1, TArg2>(TDelegate @delegate, TArg1 arg1, TArg2 arg2) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return Task.FromResult<object?>(null);

            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2>))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action<TArg1, TArg2>>(@delegate), arg1, arg2, CancellationToken.None);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2], CancellationToken.None);
        }

        public static Task<object?> InvokeTaskAsync<TDelegate, TArg1, TArg2, TArg3>(TDelegate @delegate, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action))
                throw new TargetParameterCountException();

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return Task.FromResult<object?>(null);

            if (typeof(TDelegate) == typeof(Action<TArg1, TArg2>))
                return InvokeTaskCoreAsync(messageLoopThreadId, 
                    UnsafeHelper.As<Delegate, Action<TArg1, TArg2, TArg3>>(@delegate), arg1, arg2, arg3, CancellationToken.None);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, [arg1, arg2, arg3], CancellationToken.None);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static Task<object?> InvokeTaskAsync<TDelegate>(TDelegate @delegate, params object?[]? args) where TDelegate : Delegate
            => InvokeTaskAsync(@delegate, args, CancellationToken.None);

        public static Task<object?> InvokeTaskAsync<TDelegate>(TDelegate @delegate, object?[]? args, CancellationToken cancellationToken) where TDelegate : Delegate
        {
            if (typeof(TDelegate) == typeof(Action) && args is not null && args.Length != 0)
                throw new TargetParameterCountException(nameof(args));

            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            if (messageLoopThreadId == 0)
                return Task.FromResult<object?>(null);

            if (typeof(TDelegate) == typeof(Action))
                return InvokeTaskCoreAsync(messageLoopThreadId, UnsafeHelper.As<Delegate, Action>(@delegate), cancellationToken);
            return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
        }

        private static void InvokeCoreAsync(uint threadId, Action action, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return;
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure(action, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static void InvokeCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return;
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg>(action, arg, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static void InvokeCoreAsync<TArg1, TArg2>(uint threadId,
            Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return;
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg1, TArg2>(action, arg1, arg2, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static void InvokeCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
            Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return;
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static void InvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return;
            invokeMessageFilter.AddInvoke(new InvokeClosure(@delegate, args, null, cancellationToken));
            PostInvokeMessage(threadId);
        }

        private static async Task<object?> InvokeTaskCoreAsync(uint threadId, Action action, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return null;

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure(action, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            await completionSource.Task;
            return null;
        }

        private static async Task<object?> InvokeTaskCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return null;

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg>(action, arg, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            await completionSource.Task;
            return null;
        }

        private static async Task<object?> InvokeTaskCoreAsync<TArg1, TArg2>(uint threadId,
            Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return null;

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg1, TArg2>(action, arg1, arg2, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            await completionSource.Task;
            return null;
        }

        private static async Task<object?> InvokeTaskCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
            Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return null;

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            invokeMessageFilter.AddInvoke(new SimpleInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            await completionSource.Task;
            return null;
        }

        private static Task<object?> InvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken)
        {
            InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
            if (invokeMessageFilter is null)
                return Task.FromResult<object?>(null);

            TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            invokeMessageFilter.AddInvoke(new InvokeClosure(@delegate, args, completionSource, cancellationToken));
            PostInvokeMessage(threadId);
            return completionSource.Task;
        }

        private static void PostInvokeMessage(uint threadId)
        {
            if (MathHelper.ToBoolean(InterlockedHelper.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt), true))
                return;
            User32.PostThreadMessageW(threadId, CustomWindowMessages.ConcreteWindowInvoke, 0, 0);
            InterlockedHelper.Exchange(ref _invokeBarrier, Booleans.FalseInt);
        }

        private sealed class InvokeMessageFilter : IWindowMessageFilter
        {
            public event MessageLoopExceptionEventHandler? ExceptionCaught;

            private readonly ConcurrentBag<IInvokeClosure> _invokeClosureBag = new ConcurrentBag<IInvokeClosure>();
            private readonly bool _catchExceptions;

            private int _readBarrier;

            public InvokeMessageFilter(bool catchExceptions)
            {
                _readBarrier = 0;
                _catchExceptions = catchExceptions;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddInvoke(IInvokeClosure closure) => _invokeClosureBag.Add(closure);

            public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
            {
                result = 0;
                if (hwnd != IntPtr.Zero || (uint)message != CustomWindowMessages.ConcreteWindowInvoke)
                    return false;

                ProcessAllInvoke();
                return true;
            }

            public void ProcessAllInvoke()
            {
                ConcurrentBag<IInvokeClosure> invokeClosureBag = _invokeClosureBag;

                if (InterlockedHelper.CompareExchange(ref _readBarrier, Booleans.TrueInt, Booleans.FalseInt) != Booleans.FalseInt)
                    return;

                try
                {
                    if (!invokeClosureBag.TryTake(out IInvokeClosure? closure))
                        return;

                    if (_catchExceptions)
                    {
                        do
                        {
                            try
                            {
                                closure.Invoke();
                            }
                            catch (Exception ex)
                            {
                                ExceptionCaught?.Invoke(this, new MessageLoopExceptionEventArgs(ex));
                            }
                        } while (invokeClosureBag.TryTake(out closure));
                    }
                    else
                    {
                        do
                        {
                            closure.Invoke();
                        } while (invokeClosureBag.TryTake(out closure));
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _readBarrier, Booleans.FalseInt);
                }
            }
        }
    }
}
