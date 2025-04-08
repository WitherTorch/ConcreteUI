using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class D2D1Bitmap1 : D2D1Bitmap
    {
        private new enum MethodTable
        {
            _Start = D2D1Bitmap.MethodTable._End,
            GetColorContext = _Start,
            GetOptions,
            GetSurface,
            Map,
            Unmap,
            _End
        }

        public D2D1Bitmap1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Retrieves the bitmap options used when creating the API.
        /// </summary>
        public D2D1BitmapOptions Options => GetOptions();

        /// <summary>
        /// Retrieves the color context information associated with the bitmap.
        /// </summary>
        public D2D1ColorContext GetColorContext()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetColorContext);
            ((delegate* unmanaged[Stdcall]<void*, void*, void>)functionPointer)(nativePointer, &nativePointer);
            return nativePointer == null ? null : new D2D1ColorContext(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1BitmapOptions GetOptions()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetOptions);
            return ((delegate* unmanaged[Stdcall]<void*, D2D1BitmapOptions>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Retrieves the DXGI surface from the corresponding bitmap, if the bitmap was
        /// created from a device derived from a D3D device.
        /// </summary>
        public DXGISurface GetSurface()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSurface);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, int>)functionPointer)(nativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DXGISurface(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Maps the given bitmap into memory. The bitmap must have been created with the
        /// <see cref="D2D1BitmapOptions.CpuRead"/> flag.
        /// </summary>

        [LocalsInit(false)]
        public D2D1MappedRect Map(D2D1MapOptions options)
        {
            D2D1MappedRect mappedRect;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Map);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1MapOptions, D2D1MappedRect*, int>)functionPointer)(nativePointer, options, &mappedRect);
            if (hr >= 0)
                return mappedRect;
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Unmaps the given bitmap from memory.
        /// </summary>
        public void Unmap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Unmap);
            int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}