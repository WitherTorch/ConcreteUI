using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class DXGISwapChain1 : DXGISwapChain
    {
        protected new enum MethodTable
        {
            _Start = DXGISwapChain.MethodTable._End,
            GetDesc1 = _Start,
            GetFullscreenDesc,
            GetHwnd,
            GetCoreWindow,
            Present1,
            IsTemporaryMonoSupported,
            GetRestrictToOutput,
            SetBackgroundColor,
            GetBackgroundColor,
            SetRotation,
            GetRotation,
            _End
        }

        public DXGISwapChain1() : base() { }

        public DXGISwapChain1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public DXGISwapChainDescription1 Description1
        {
            [LocalsInit(false)]
            get => GetDesc1();
        }

        public DXGISwapChainFullscreenDescription FullscreenDescription
        {
            [LocalsInit(false)]
            get => GetFullscreenDesc();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGISwapChainDescription1 GetDesc1()
        {
            DXGISwapChainDescription1 desc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc1);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGISwapChainDescription1*, int>)functionPointer)(nativePointer, &desc);
            if (hr >= 0)
                return desc;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private DXGISwapChainFullscreenDescription GetFullscreenDesc()
        {
            DXGISwapChainFullscreenDescription fullscreenDesc;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFullscreenDesc);
            int hr = ((delegate* unmanaged[Stdcall]<void*, DXGISwapChainFullscreenDescription*, int>)functionPointer)(nativePointer, &fullscreenDesc);
            if (hr >= 0)
                return fullscreenDesc;
            throw Marshal.GetExceptionForHR(hr);
        }

        public IntPtr GetHwnd()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetHwnd);
            int hr = ((delegate* unmanaged[Stdcall]<void*, IntPtr*, int>)functionPointer)(nativePointer, (IntPtr*)&nativePointer);
            if (hr >= 0)
                return (IntPtr)nativePointer;
            throw Marshal.GetExceptionForHR(hr);
        }

        public void Present1(uint syncInterval, in DXGIPresentParameters presentParameters)
            => Present1(syncInterval, DXGIPresentFlags.None, presentParameters);

        public void Present1(uint syncInterval, DXGIPresentFlags flags, in DXGIPresentParameters presentParameters)
        {
            int hr = TryPresent1(syncInterval, flags, presentParameters);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        public void Present1(uint syncInterval, DXGIPresentFlags flags, DXGIPresentParameters* pPresentParameters)
        {
            int hr = TryPresent1(syncInterval, flags, pPresentParameters);
            if (hr >= 0)
                return;
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public int TryPresent1(uint syncInterval, in DXGIPresentParameters presentParameters)
            => TryPresent1(syncInterval, DXGIPresentFlags.None, presentParameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TryPresent1(uint syncInterval, DXGIPresentFlags flags, in DXGIPresentParameters presentParameters)
            => TryPresent1(syncInterval, flags, UnsafeHelper.AsPointerIn(in presentParameters));

        public int TryPresent1(uint syncInterval, DXGIPresentFlags flags, DXGIPresentParameters* pPresentParameters)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Present1);
            return ((delegate* unmanaged[Stdcall]<void*, uint, DXGIPresentFlags, DXGIPresentParameters*, int>)functionPointer)(nativePointer,
                syncInterval, flags, pPresentParameters);
        }
    }
}
