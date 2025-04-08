using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;
using WitherTorch.Common.Windows;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGISwapChain : DXGIDeviceSubObject
    {
        protected new enum MethodTable
        {
            _Start = DXGIDeviceSubObject.MethodTable._End,
            Present = _Start,
            GetBuffer,
            SetFullscreenState,
            GetFullscreenState,
            GetDesc,
            ResizeBuffers,
            ResizeTarget,
            GetContainingOutput,
            GetFrameStatistics,
            GetLastPresentCount,
            _End
        }

        public DXGISwapChain() : base() { }

        public DXGISwapChain(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGISwapChainDescription Description
        {
            [LocalsInit(false)]
            get => GetDesc();
        }

        public void Present(uint syncInterval, DXGIPresentFlags flags)
        {
            int hr = TryPresent(syncInterval, flags);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        public int TryPresent(uint syncInterval, DXGIPresentFlags flags)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Present);
            return ((delegate* unmanaged[Stdcall]<void*, uint, DXGIPresentFlags, int>)functionPointer)(nativePointer, syncInterval, flags);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetBuffer<T>(uint buffer, in Guid riid) where T : ComObject, new()
            => GetBuffer<T>(buffer, UnsafeHelper.AsPointerIn(in riid));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComObject GetBuffer(uint buffer, in Guid riid)
            => GetBuffer(buffer, UnsafeHelper.AsPointerIn(in riid));

        public T GetBuffer<T>(uint buffer, Guid* riid) where T : ComObject, new()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBuffer);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, Guid*, void**, int>)functionPointer)(nativePointer, buffer, riid, &nativePointer);
            if (hr >= 0)
                return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        public ComObject GetBuffer(uint buffer, Guid* riid)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBuffer);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, Guid*, void**, int>)functionPointer)(nativePointer, buffer, riid, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGISwapChainDescription GetDesc()
        {
            DXGISwapChainDescription desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBuffer);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGISwapChainDescription*, int>)functionPointer)(nativePointer, &desc);
            if (hr >= 0)
                return desc;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void ResizeBuffers()
            => ResizeBuffers(0, 0, 0, DXGIFormat.Unknown, DXGISwapChainFlags.None);

        [Inline(InlineBehavior.Keep, export: true)]
        public void ResizeBuffers(uint bufferCount, uint width, uint height)
            => ResizeBuffers(bufferCount, width, height, DXGIFormat.Unknown, DXGISwapChainFlags.None);

        public void ResizeBuffers(uint bufferCount, uint width, uint height, DXGIFormat format, DXGISwapChainFlags flags)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ResizeBuffers);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint, uint, DXGIFormat, DXGISwapChainFlags, int>)functionPointer)(nativePointer,
                bufferCount, width, height, format, flags);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }
    }
}
