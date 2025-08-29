using System;
using System.Runtime.ConstrainedExecution;

using ConcreteUI.Native;

namespace ConcreteUI.Utils
{
    public sealed class Win32ImageHandle : CriticalFinalizerObject, IDisposable
    {
        private IntPtr _handle;
        private Win32ImageType _type;

        public IntPtr Handle => _handle;

        public Win32ImageHandle(IntPtr handle, Win32ImageType type, bool ownsHandle)
        {
            if (ownsHandle && handle != IntPtr.Zero)
            {
                if (type >= Win32ImageType.EmhMetafile)
                    throw new ArgumentOutOfRangeException(nameof(type));
                _handle = handle;
                _type = type;
                return;
            }
            _handle = handle;
            GC.SuppressFinalize(this);
            _type = (Win32ImageType)uint.MaxValue;
        }

        ~Win32ImageHandle() => DisposeCore();

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }

        private void DisposeCore()
        {
            Win32ImageType type = _type;
            if (type >= Win32ImageType.EmhMetafile)
                return;
            _type = (Win32ImageType)uint.MaxValue;

            IntPtr handle = _handle;
            _handle = IntPtr.Zero;
            if (handle == IntPtr.Zero)
                return;

            switch (type)
            {
                case Win32ImageType.Bitmap:
                    Gdi32.DeleteObject(handle);
                    break;
                case Win32ImageType.Icon:
                    User32.DestroyIcon(handle);
                    break;
                case Win32ImageType.Cursor:
                    User32.DestroyCursor(handle);
                    break;
            }
        }
    }

    public enum Win32ImageType : uint
    {
        Bitmap,
        Icon,
        Cursor,
        EmhMetafile
    }
}
