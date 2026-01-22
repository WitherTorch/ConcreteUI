using System;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public abstract class DisposableUIElementBase : UIElement, ICheckableDisposable
    {
        private bool _disposed;

        protected DisposableUIElementBase(IRenderer renderer, string themePrefix)
            : base(renderer, themePrefix)
        {
            _disposed = false;
        }

        public bool IsDisposed => _disposed;

        protected abstract void DisposeCore(bool disposing);

        private void Dispose(bool disposing)
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            DisposeCore(disposing);
        }

        ~DisposableUIElementBase() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
