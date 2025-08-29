using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private record class SimpleInvokeClosure(
            Action Action,
            TaskCompletionSource<bool>? CompletionSource,
            CancellationToken CancellationToken) : IInvokeClosure
        {
            public void Invoke()
            {
                TaskCompletionSource<bool>? completionSource = CompletionSource;
                if (completionSource is null)
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;
                    Action.Invoke();
                    return;
                }
                if (CancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled();
                    return;
                }
                try
                {
                    Action.Invoke();
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    return;
                }
                completionSource.TrySetResult(true);
            }
        }
    }
}
