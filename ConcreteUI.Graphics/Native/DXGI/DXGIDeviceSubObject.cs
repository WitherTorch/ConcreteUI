using System;
using System.Runtime.CompilerServices;
using System.Security;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public abstract unsafe class DXGIDeviceSubObject : DXGIObject
    {
        protected new enum MethodTable
        {
            _Start = DXGIObject.MethodTable._End,
            GetDevice = _Start,
            _End
        }

        public DXGIDeviceSubObject() { }
        public DXGIDeviceSubObject(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetDevice<T>(in Guid riid, bool throwException = true) where T : ComObject, new()
            => GetDevice<T>(UnsafeHelper.AsPointerIn(in riid), throwException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComObject? GetDevice(in Guid riid, bool throwException = true)
            => GetDevice(UnsafeHelper.AsPointerIn(in riid), throwException);

        public T? GetDevice<T>(Guid* riid, bool throwException = true) where T : ComObject, new()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDevice);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
        }

        public ComObject? GetDevice(Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDevice);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
        }
    }
}
