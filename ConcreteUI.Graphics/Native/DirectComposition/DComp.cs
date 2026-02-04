using System;

using WitherTorch.Common.Windows.Helpers;

namespace ConcreteUI.Graphics.Native.DirectComposition
{
    public static unsafe class DComp
    {
        private const string LibraryName = "Dcomp.dll";

        private static readonly void* _pointer;

        static DComp()
        {
            _pointer = MethodImportHelper.GetImportedMethodPointer(LibraryName, nameof(DCompositionCreateDevice));
        }

        public static int DCompositionCreateDevice(void* dxgiDevice, Guid* iid, void** dcompositionDevice)
        {
            void* pointer = _pointer;
            if (pointer is null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)pointer)(dxgiDevice, iid, dcompositionDevice);
        }
    }
}
