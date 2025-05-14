using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICBitmapFrameDecode : WICBitmapSource
    {
        private new enum MethodTable
        {
            _Start = WICBitmapSource.MethodTable._End,
            GetMetadataQueryReader = _Start,
            GetColorContexts,
            GetThumbnail,
            _End,
        }

        public WICBitmapFrameDecode() : base() { }
        public WICBitmapFrameDecode(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public WICBitmapSource? GetThumbnail()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetThumbnail);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new WICBitmapSource(nativePointer, ReferenceType.Owned);
        }
    }
}
