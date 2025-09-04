using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private abstract class InvokeClosureBase<TDelegate, TResult> : IInvokeClosure where TDelegate : Delegate
        {
            private readonly TDelegate _delegate;
            private readonly TaskCompletionSource<TResult>? _completionSource;
            private readonly CancellationToken _cancellationToken;

            protected InvokeClosureBase(TDelegate @delegate, TaskCompletionSource<TResult>? completionSource, CancellationToken cancellationToken)
            {
                _delegate = @delegate;
                _completionSource = completionSource;
                _cancellationToken = cancellationToken;
            }

            public void Invoke()
            {
                TaskCompletionSource<TResult>? completionSource = _completionSource;
                if (completionSource is null)
                    InvokeSimple();
                else
                    InvokeFull(completionSource);
            }

            public void InvokeSimple()
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;
                TResult result = InvokeCore(_delegate);
                (result as IDisposable)?.Dispose();
            }

            public void InvokeFull(TaskCompletionSource<TResult> completionSource)
            {
                CancellationToken cancellationToken = _cancellationToken;
                if (cancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                    return;
                }
                TResult result;
                try
                {
                    result = InvokeCore(_delegate);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    return;
                }
                completionSource.TrySetResult(result);
            }

            protected abstract TResult InvokeCore(TDelegate invoker);
        }
    }
}
