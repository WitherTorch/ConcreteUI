using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ConcreteUI.Internals;
using ConcreteUI.Window;

using WitherTorch.Common;
using WitherTorch.Common.Buffers;
using WitherTorch.Common.Collections;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;


#if NET472_OR_GREATER
using WitherTorch.Common.Extensions;
#endif

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private class InvokeMessageFilter : IWindowMessageFilter
        {
            private readonly SwapQueue<IInvokeClosure> _invokeClosureQueue = new SwapQueue<IInvokeClosure>();

            private int _readBarrier;

            public InvokeMessageFilter() => _readBarrier = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddInvoke(IInvokeClosure closure)
            {
                Queue<IInvokeClosure> queue = _invokeClosureQueue.Value;
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
                if (InterlockedHelper.CompareExchange(ref _readBarrier, Booleans.TrueInt, Booleans.FalseInt) != Booleans.FalseInt)
                    return;

                Queue<IInvokeClosure> queue = _invokeClosureQueue.Swap();
                try
                {
                    lock (queue)
                    {
                        while (queue.TryDequeue(out IInvokeClosure? closure))
                        {
                            if (closure is not null)
                                DoInvoke(closure);
                        }
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
