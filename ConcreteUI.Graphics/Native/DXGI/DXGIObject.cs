using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public abstract unsafe class DXGIObject : ComObject
    {
        protected new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            SetPrivateData = _Start,
            SetPrivateDataInterface,
            GetPrivateData,
            GetParent,
            _End
        }

        public DXGIObject() : base() { }

        public DXGIObject(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public void SetPrivateData<T>(in Guid name, T data) where T : unmanaged
            => SetPrivateData(name, unchecked((uint)sizeof(T)), &data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrivateData(in Guid name, uint dataSize, void* pData)
            => SetPrivateData(UnsafeHelper.AsPointerIn(in name), dataSize, pData);

        public void SetPrivateData(Guid* name, uint dataSize, void* pData)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetPrivateData);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, uint, void*, int>)functionPointer)(nativePointer, name, dataSize, pData);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrivateDataInterface(in Guid name, ComObject value)
            => SetPrivateDataInterface(UnsafeHelper.AsPointerIn(in name), value);

        public void SetPrivateDataInterface(Guid* name, ComObject value)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetPrivateDataInterface);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void*, int>)functionPointer)(nativePointer, name, value == null ? null : value.NativePointer);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPrivateData(in Guid name, uint* pDataSize, void* pData)
            => GetPrivateData(UnsafeHelper.AsPointerIn(in name), pDataSize, pData);

        public void GetPrivateData(Guid* name, uint* pDataSize, void* pData)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPrivateData);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, uint*, void*, int>)functionPointer)(nativePointer, name, pDataSize, pData);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetParent<T>(in Guid riid, bool throwException = true) where T : ComObject, new()
            => GetParent<T>(UnsafeHelper.AsPointerIn(in riid), throwException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComObject GetParent(in Guid riid, bool throwException = true)
            => GetParent(UnsafeHelper.AsPointerIn(in riid), throwException);

        public T GetParent<T>(Guid* riid, bool throwException = true) where T : ComObject, new()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (hr >= 0)
                return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }

        public ComObject GetParent(Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
            if (throwException)
                throw Marshal.GetExceptionForHR(hr);
            return null;
        }
    }
}
