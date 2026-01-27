using System;
using System.Security;

using ConcreteUI.Graphics;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Internals.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class ShCore
    {
        private const string LibraryName = "Shcore.dll";
        private static readonly void*[] _pointers = MethodImportHelper.GetImportedMethodPointers(LibraryName,
            nameof(GetDpiForMonitor));

        [LocalsInit(false)]
        public static int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY)
        {
            UnsafeHelper.SkipInit(out dpiX);
            UnsafeHelper.SkipInit(out dpiY);
            void* pointer = _pointers[0];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
                [Stdcall, SuppressGCTransition]
#else
                [Stdcall]
#endif
                <IntPtr, MonitorDpiType, uint*, uint*, int>)pointer)(hMonitor, dpiType, UnsafeHelper.AsPointerOut(out dpiX), UnsafeHelper.AsPointerOut(out dpiY));
        }
    }
}
