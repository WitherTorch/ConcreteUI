using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICBitmapScaler : WICBitmapSource
    {
        public WICBitmapScaler() : base() { }

        public WICBitmapScaler(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = WICBitmapSource.MethodTable._End,
            Initialize = _Start,
            _End,
        }

        public void Initialize(WICBitmapSource source, uint width, uint height, WICBitmapInterpolationMode mode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Initialize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, uint, uint, WICBitmapInterpolationMode, int>)functionPointer)(nativePointer, 
                source.NativePointer, width, height, mode);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
