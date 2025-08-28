using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcreteUI.Window2
{
    partial class NativeWindow
    {
        private record class InvokeClosure(
            Delegate Delegate,
            object?[]? Arguments,
            TaskCompletionSource<object?>? CompletionSource,
            CancellationToken CancellationToken)
        {
            public void Invoke()
            {
                TaskCompletionSource<object?>? completionSource = CompletionSource;
                if (completionSource is null)
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;
                    Delegate.DynamicInvoke(Arguments);
                    return;
                }
                if (CancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled();
                    return;
                }
                object? result;
                try
                {
                    result = Delegate.DynamicInvoke(Arguments);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    return;
                }
                completionSource.TrySetResult(result);
            }
        }
    }
}
