using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Internals;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Buffers;
using WitherTorch.Common.Helpers;

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private class InvokeMessageFilter : IWindowMessageFilter
        {
            private readonly Queue<IInvokeClosure> _invokeClosureQueue = new Queue<IInvokeClosure>();

            private int _readBarrier;

            public InvokeMessageFilter() => _readBarrier = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddInvoke(IInvokeClosure closure)
            {
                Queue<IInvokeClosure> queue = _invokeClosureQueue;
                lock (queue)
                    queue.Enqueue(closure);
            }

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
                ArrayPool<IInvokeClosure> pool = ArrayPool<IInvokeClosure>.Shared;
                Queue<IInvokeClosure> queue = _invokeClosureQueue;

                if (InterlockedHelper.CompareExchange(ref _readBarrier, Booleans.TrueInt, Booleans.FalseInt) != Booleans.FalseInt)
                    return;

                try
                {
                    IInvokeClosure[] buffer;
                    int count;

                    lock (queue)
                    {
                        count = queue.Count;
                        if (count <= 0)
                            return;
                        buffer = pool.Rent(count);
                        try
                        {
                            queue.CopyTo(buffer, 0); // 建立快照並快速清空佇列
                            queue.Clear();
                        }
                        catch (Exception)
                        {
                            pool.Return(buffer);
                            throw;
                        }
                    }

                    ref IInvokeClosure bufferRef = ref buffer[0];
                    try
                    {
                        for (nuint i = 0, limit = (nuint)count; i < limit; i++)
                        {
                            IInvokeClosure closure = UnsafeHelper.AddByteOffset(ref bufferRef, i * UnsafeHelper.SizeOf<IInvokeClosure>());
                            DoInvoke(closure);
                        }
                    }
                    finally
                    {
                        pool.Return(buffer);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _readBarrier, Booleans.FalseInt);
                }
            }

            protected virtual void DoInvoke(IInvokeClosure closure) => closure.Invoke();
        }

        private sealed class InvokeMessageFilterSafe : InvokeMessageFilter
        {
            public InvokeMessageFilterSafe() { }

            protected override void DoInvoke(IInvokeClosure closure)
            {
                try
                {
                    base.DoInvoke(closure);
                }
                catch (Exception ex)
                {
                    ExceptionCaught?.Invoke(this, new MessageLoopExceptionEventArgs(ex));
                }
            }
        }
    }
}
