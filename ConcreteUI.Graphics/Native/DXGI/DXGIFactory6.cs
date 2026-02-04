using System;
using System.Runtime.CompilerServices;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory6 : DXGIFactory5
    {
        public static readonly Guid IID_IDXGIFactory6 = new Guid(0xc1b6694f, 0xff09, 0x44a9, 0xb0, 0x3c, 0x77, 0x90, 0x0a, 0x0a, 0x1d, 0x17);

        private new enum MethodTable
        {
            _Start = DXGIFactory5.MethodTable._End,
            EnumAdapterByGpuPreference = _Start,
            _End
        }

        public DXGIFactory6() : base() { }

        public DXGIFactory6(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DXGIAdapter? EnumAdapterByGpuPreference(uint adapter, DXGIGpuPreference perference, in Guid riid, bool throwException = true)
            => EnumAdapterByGpuPreference(adapter, perference, UnsafeHelper.AsPointerIn(in riid), throwException);

        public DXGIAdapter? EnumAdapterByGpuPreference(uint adapter, DXGIGpuPreference perference, Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapterByGpuPreference);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, DXGIGpuPreference, Guid*, void**, int>)functionPointer)(nativePointer, adapter, perference, riid, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return nativePointer == null ? null : new DXGIAdapter(nativePointer, ReferenceType.Owned);
        }
    }
}
