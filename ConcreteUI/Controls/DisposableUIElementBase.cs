using WitherTorch.Common;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public abstract class DisposableUIElementBase : UIElement, ISafeDisposable
    {
        private bool _disposed;

        protected DisposableUIElementBase(IRenderer renderer, string themePrefix)
            : base(renderer, themePrefix)
        {
            _disposed = false;
        }

        public bool IsDisposed => _disposed;

        protected abstract void DisposeCore(bool disposing);

        bool ISafeDisposable.MarkAsDisposed() => ReferenceHelper.Exchange(ref _disposed, true);

        void ISafeDisposable.DisposeCore(bool disposing) => DisposeCore(disposing);

        ~DisposableUIElementBase() => SafeDisposableDefaults.Finalize(this);

        public void Dispose() => SafeDisposableDefaults.Dispose(this);
    }
}
