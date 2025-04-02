using System;

namespace ConcreteUI.Graphics.Hosting
{
    public abstract partial class GraphicsHostBase : IDisposable
    {
        public abstract bool IsDisposed { get; }

        public IntPtr Handle { get; }

        protected GraphicsHostBase(IntPtr handle)
        {
            Handle = handle;
        }

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
