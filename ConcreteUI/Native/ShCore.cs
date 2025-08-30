using System;
using System.Security;

using ConcreteUI.Graphics;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class ShCore
    {
        private const string SHCORE_DLL = "Shcore.dll";
        private static readonly void*[] _pointers = MethodImportHelper.GetImportedMethodPointers(SHCORE_DLL,
            nameof(GetDpiForMonitor));

        [LocalsInit(false)]
        public static int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY)
        {
            UnsafeHelper.SkipInit(out dpiX);
            UnsafeHelper.SkipInit(out dpiY);
            void* pointer = _pointers[0];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<IntPtr, MonitorDpiType, uint*, uint*, int>)pointer)(
                hMonitor, dpiType, UnsafeHelper.AsPointerOut(out dpiX), UnsafeHelper.AsPointerOut(out dpiY));
        }
    }
}
