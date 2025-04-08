using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory4 : DXGIFactory3
    {
        public static readonly Guid IID_DXGIFactory4 = new Guid(0x1bc6ea02, 0xef36, 0x464f, 0xbf, 0x0c, 0x21, 0xca, 0x39, 0xe5, 0x16, 0x8a);

        protected new enum MethodTable
        {
            _Start = DXGIFactory3.MethodTable._End,
            EnumAdapterByLuid = _Start,
            EnumWarpAdapter,
            _End,
        }

        public DXGIFactory4() : base() { }

        public DXGIFactory4(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DXGIAdapter EnumAdapterByLuid(Luid adapterLuid, in Guid riid, bool throwException = true)
            => EnumAdapterByLuid(adapterLuid, UnsafeHelper.AsPointerIn(in riid), throwException);

        public DXGIAdapter EnumAdapterByLuid(Luid adapterLuid, Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapterByLuid);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Luid, Guid*, void**, int>)functionPointer)(nativePointer, adapterLuid, riid, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DXGIAdapter(nativePointer, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DXGIAdapter EnumWarpAdapter(in Guid riid, bool throwException = true)
            => EnumWarpAdapter(UnsafeHelper.AsPointerIn(in riid), throwException);

        public DXGIAdapter EnumWarpAdapter(Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumWarpAdapter);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new DXGIAdapter(nativePointer, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }
    }
}
