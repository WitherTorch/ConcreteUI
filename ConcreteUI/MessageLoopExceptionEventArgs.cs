using System;

namespace ConcreteUI
{
    public delegate void MessageLoopExceptionEventHandler(object? sender, MessageLoopExceptionEventArgs e);

    public sealed class MessageLoopExceptionEventArgs : EventArgs
    {
        private readonly Exception _exception;

        public Exception Exception => _exception;

        public MessageLoopExceptionEventArgs(Exception exception) => _exception = exception;
    }
}