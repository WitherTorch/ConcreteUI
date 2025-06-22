using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICBitmapFlipRotator : WICBitmapSource
    {
        public WICBitmapFlipRotator() : base() { }

        public WICBitmapFlipRotator(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = WICBitmapSource.MethodTable._End,
            Initialize = _Start,
            _End,
        }

        public void Initialize(WICBitmapSource source, WICBitmapTransformOptions options)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Initialize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, WICBitmapTransformOptions, int>)functionPointer)(nativePointer, 
                source.NativePointer, options);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
