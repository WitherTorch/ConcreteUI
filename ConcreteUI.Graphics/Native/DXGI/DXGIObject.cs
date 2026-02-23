using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;

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
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrivateDataInterface(in Guid name, ComObject value)
            => SetPrivateDataInterface(UnsafeHelper.AsPointerIn(in name), value);

        public void SetPrivateDataInterface(Guid* name, ComObject value)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetPrivateDataInterface);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void*, int>)functionPointer)(nativePointer, name, value == null ? null : value.NativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPrivateData(in Guid name, uint* pDataSize, void* pData)
            => GetPrivateData(UnsafeHelper.AsPointerIn(in name), pDataSize, pData);

        public void GetPrivateData(Guid* name, uint* pDataSize, void* pData)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPrivateData);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, uint*, void*, int>)functionPointer)(nativePointer, name, pDataSize, pData);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetParent<T>(in Guid riid, bool throwException = true) where T : ComObject, new()
            => GetParent<T>(UnsafeHelper.AsPointerIn(in riid), throwException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComObject? GetParent(in Guid riid, bool throwException = true)
            => GetParent(UnsafeHelper.AsPointerIn(in riid), throwException);

        public T? GetParent<T>(Guid* riid, bool throwException = true) where T : ComObject, new()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
        }

        public ComObject? GetParent(Guid* riid, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
        }
    }
}
