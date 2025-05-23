﻿using System;
using System.Runtime.CompilerServices;
using System.Security;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGIFactory : DXGIObject
    {
        public static readonly Guid IID_DXGIFactory = new Guid(0x7b7166ec, 0x21c7, 0x44ae, 0xb2, 0x1a, 0xc9, 0xae, 0x32, 0x1a, 0xe3, 0x69);

        protected new enum MethodTable
        {
            _Start = DXGIObject.MethodTable._End,
            EnumAdapters = _Start,
            MakeWindowAssociation,
            GetWindowAssociation,
            CreateSwapChain,
            CreateSoftwareAdapter,
            _End
        }

        public DXGIFactory() : base() { }

        public DXGIFactory(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DXGIFactory? Create(in Guid riid, bool throwException = true)
            => Create(UnsafeHelper.AsPointerIn(in riid), throwException);

        [LocalsInit(false)]
        public static DXGIFactory? Create(Guid* riid, bool throwException = true)
        {
            void* factory;
            int hr = DXGI.CreateDXGIFactory(riid, &factory);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref factory);
            return factory == null ? null : new DXGIFactory(factory, ReferenceType.Owned);
        }

        public DXGIAdapter? EnumAdapters(uint adapter, bool throwException = true)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapters);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, adapter, &nativePointer);
            if (throwException)
                ThrowHelper.ThrowExceptionForHR(hr);
            else
                ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
            return nativePointer == null ? null : new DXGIAdapter(nativePointer, ReferenceType.Owned);
        }

        public void MakeWindowAssociation(IntPtr windowHandle, DXGIMakeWindowAssociationFlags flags)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.MakeWindowAssociation);
            int hr = ((delegate* unmanaged[Stdcall]<void*, IntPtr, DXGIMakeWindowAssociationFlags, int>)functionPointer)(nativePointer, windowHandle, flags);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        public IntPtr GetWindowAssociation()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetWindowAssociation);
            int hr = ((delegate* unmanaged[Stdcall]<void*, IntPtr*, int>)functionPointer)(nativePointer, (IntPtr*)&nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
            return (IntPtr)nativePointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DXGISwapChain CreateSwapChain(ComObject device, in DXGISwapChainDescription desc)
            => CreateSwapChain(device, UnsafeHelper.AsPointerIn(in desc));

        public DXGISwapChain CreateSwapChain(ComObject device, DXGISwapChainDescription* pDesc)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapters);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, DXGISwapChainDescription*, void**, int>)functionPointer)(nativePointer,
                device.NativePointer, pDesc, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new DXGISwapChain(nativePointer, ReferenceType.Owned);
        }
    }
}
