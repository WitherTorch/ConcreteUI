using System.Drawing;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICBitmapClipper : WICBitmapSource
    {
        public WICBitmapClipper() : base() { }

        public WICBitmapClipper(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        private new enum MethodTable
        {
            _Start = WICBitmapSource.MethodTable._End,
            Initialize = _Start,
            _End,
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(WICBitmapSource source, in Rectangle rect)
            => Initialize(source, UnsafeHelper.AsPointerIn(in rect));

        public void Initialize(WICBitmapSource source, Rectangle* pRect)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Initialize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, Rectangle*, int>)functionPointer)(nativePointer, 
                source.NativePointer, pRect);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
