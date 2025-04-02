using System;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.CrossNative;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class DXGISurface : DXGIDeviceSubObject
    {
        public static readonly Guid IID_DXGISurface = new Guid(0xcafcb56c, 0x6ac3, 0x4889, 0xbf, 0x47, 0x9e, 0x23, 0xbb, 0xd2, 0x60, 0xec);

        private new enum MethodTable
        {
            _Start = DXGIDeviceSubObject.MethodTable._End,
            GetDesc = _Start,
            Map,
            Unmap,
            _End
        }

        public DXGISurface() : base() { }

        public DXGISurface(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGISurfaceDescription Description
        {
            [LocalsInit(false)]
            get => GetDesc();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGISurfaceDescription GetDesc()
        {
            DXGISurfaceDescription desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc);
            int hr = ((delegate*<void*, DXGISurfaceDescription*, int>)functionPointer)(nativePointer, &desc);
            if (hr >= 0)
                return desc;
            throw Marshal.GetExceptionForHR(hr);
        }

        [LocalsInit(false)]
        public DXGIMappedRect Map(DXGIMapFlags flags)
        {
            DXGIMappedRect lockedRect;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Map);
            int hr = ((delegate*<void*, DXGIMappedRect*, DXGIMapFlags, int>)functionPointer)(nativePointer, &lockedRect, flags);
            if (hr >= 0)
                return lockedRect;
            throw Marshal.GetExceptionForHR(hr);
        }

        public void Unmap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Unmap);
            int hr = ((delegate*<void*, int>)functionPointer)(nativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
