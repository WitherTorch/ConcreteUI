using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcreteUI
{
    partial class WindowMessageLoop
    {
        private abstract class SimpleInvokeClosureBase<TDelegate> : InvokeClosureBase<TDelegate, bool> where TDelegate : Delegate
        {
            protected SimpleInvokeClosureBase(TDelegate @delegate, TaskCompletionSource<bool>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken) { }

            protected override bool InvokeCore(TDelegate invoker)
            {
                InvokeCoreWithNoReturn(invoker);
                return true;
            }

            protected abstract void InvokeCoreWithNoReturn(TDelegate invoker);
        }

        private sealed class SimpleInvokeClosure : SimpleInvokeClosureBase<Action>
        {
            public SimpleInvokeClosure(Action @delegate, TaskCompletionSource<bool>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken) { }

            protected override void InvokeCoreWithNoReturn(Action invoker) => invoker.Invoke();
        }

        private sealed class SimpleInvokeClosure<TArg> : SimpleInvokeClosureBase<Action<TArg>>
        {
            private readonly TArg _arg;

            public SimpleInvokeClosure(Action<TArg> @delegate, TArg arg, 
                TaskCompletionSource<bool>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken)
            {
                _arg = arg;
            }

            protected override void InvokeCoreWithNoReturn(Action<TArg> invoker) => invoker.Invoke(_arg);
        }

        private sealed class SimpleInvokeClosure<TArg1, TArg2> : SimpleInvokeClosureBase<Action<TArg1, TArg2>>
        {
            private readonly TArg1 _arg1;
            private readonly TArg2 _arg2;

            public SimpleInvokeClosure(Action<TArg1, TArg2> @delegate, TArg1 arg1, TArg2 arg2, 
                TaskCompletionSource<bool>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken)
            {
                _arg1 = arg1;
                _arg2 = arg2;
            }

            protected override void InvokeCoreWithNoReturn(Action<TArg1, TArg2> invoker) => invoker.Invoke(_arg1, _arg2);
        }

        private sealed class SimpleInvokeClosure<TArg1, TArg2, TArg3> : SimpleInvokeClosureBase<Action<TArg1, TArg2, TArg3>>
        {
            private readonly TArg1 _arg1;
            private readonly TArg2 _arg2;
            private readonly TArg3 _arg3;

            public SimpleInvokeClosure(Action<TArg1, TArg2, TArg3> @delegate, TArg1 arg1, TArg2 arg2, TArg3 arg3, 
                TaskCompletionSource<bool>? completionSource, CancellationToken cancellationToken)
                : base(@delegate, completionSource, cancellationToken)
            {
                _arg1 = arg1;
                _arg2 = arg2;
                _arg3 = arg3;
            }

            protected override void InvokeCoreWithNoReturn(Action<TArg1, TArg2, TArg3> invoker) => invoker.Invoke(_arg1, _arg2, _arg3);
        }
    }
}
