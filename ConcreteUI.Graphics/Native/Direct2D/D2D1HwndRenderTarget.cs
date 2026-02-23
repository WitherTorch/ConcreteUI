using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Renders drawing instructions to a window.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class D2D1HwndRenderTarget : D2D1RenderTarget
    {
        public static readonly Guid IID_IHwndRenderTarget = new Guid(0x2cd90698, 0x12e2, 0x11dc, 0x9f, 0xed, 0x00, 0x11, 0x43, 0xa0, 0x55, 0xf9);

        private new enum MethodTable
        {
            _Start = D2D1RenderTarget.MethodTable._End,
            CheckWindowState = _Start,
            Resize,
            GetHwnd,
            _End
        }

        public IntPtr Hwnd
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetHwnd();
        }

        public D2D1HwndRenderTarget() : base() { }

        public D2D1HwndRenderTarget(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1WindowState CheckWindowState()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CheckWindowState);
            return ((delegate* unmanaged[Stdcall]<void*, D2D1WindowState>)functionPointer)(nativePointer);
        }

        public void Resize(in SizeU size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Resize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, SizeU*, int>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(size));
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private IntPtr GetHwnd()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetHwnd);
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, IntPtr>)functionPointer)(nativePointer);
        }
    }
}
