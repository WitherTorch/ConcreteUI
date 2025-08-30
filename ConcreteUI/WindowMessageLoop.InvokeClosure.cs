using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private sealed class InvokeClosure : InvokeClosureBase<Delegate, object?>
        {
            private readonly object?[]? _args;

            public InvokeClosure(Delegate @delegate, object?[]? args,
                TaskCompletionSource<object?>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken)
            {
                _args = args;
            }

            protected override object? InvokeCore(Delegate invoker)
                => invoker.DynamicInvoke(_args);
        }
    }
}
