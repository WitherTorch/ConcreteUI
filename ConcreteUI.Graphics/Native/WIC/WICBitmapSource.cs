using System;
using System.Drawing;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.WIC
{
    public unsafe class WICBitmapSource : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            GetSize = _Start,
            GetPixelFormat,
            GetResolution,
            CopyPalette,
            CopyPixels,
            _End,
        }

        public WICBitmapSource() : base() { }
        public WICBitmapSource(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public Size Size => GetSize();
        public Guid PixelFormat => GetPixelFormat();
        public PointD Resolution => GetResolution();


        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private Size GetSize()
        {
            Size result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int*, int*, int>)functionPointer)(nativePointer, (int*)&result, (int*)&result + 1);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private Guid GetPixelFormat()
        {
            Guid result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPixelFormat);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private PointD GetResolution()
        {
            PointD result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetResolution);
            int hr = ((delegate* unmanaged[Stdcall]<void*, double*, double*, int>)functionPointer)(nativePointer, (double*)&result, (double*)&result + 1);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        public void CopyPixels(in Rectangle rect, uint stride, byte* buffer, uint bufferSize)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CopyPixels);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Rectangle*, uint, uint, byte*, int>)functionPointer)(nativePointer,
                UnsafeHelper.AsPointerIn(rect), stride, bufferSize, buffer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}
