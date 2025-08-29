using System;
using System.Runtime.ConstrainedExecution;

namespace ConcreteUI.Native
{
    public sealed class Win32ImageHandle : CriticalFinalizerObject, IDisposable
    {
        private IntPtr _handle;
        private ImageType _type;

        public IntPtr Handle => _handle;

        public Win32ImageHandle(IntPtr handle, ImageType type, bool ownsHandle)
        {
            if (ownsHandle && handle != IntPtr.Zero)
            {
                if (type >= ImageType.EmhMetafile)
                    throw new ArgumentOutOfRangeException(nameof(type));
                _handle = handle;
                _type = type;
                return;
            }
            _handle = handle;
            GC.SuppressFinalize(this);
            _type = (ImageType)uint.MaxValue;
        }

        ~Win32ImageHandle() => DisposeCore();

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }

        private void DisposeCore()
        {
            ImageType type = _type;
            if (type >= ImageType.EmhMetafile)
                return;
            _type = (ImageType)uint.MaxValue;

            IntPtr handle = _handle;
            _handle = IntPtr.Zero;
            if (handle == IntPtr.Zero)
                return;

            switch (type)
            {
                case ImageType.Bitmap:
                    Gdi32.DeleteObject(handle);
                    break;
                case ImageType.Icon:
                    User32.DestroyIcon(handle);
                    break;
                case ImageType.Cursor:
                    User32.DestroyCursor(handle);
                    break;
            }
        }
    }
}
