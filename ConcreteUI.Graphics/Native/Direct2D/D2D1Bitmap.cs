using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe class D2D1Bitmap : D2D1Image
    {
        protected new enum MethodTable
        {
            _Start = D2D1Image.MethodTable._End,
            GetSize = _Start,
            GetPixelSize,
            GetPixelFormat,
            GetDpi,
            CopyFromBitmap,
            CopyFromRenderTarget,
            CopyFromMemory,
            _End
        }

        public D2D1Bitmap() : base() { }

        public D2D1Bitmap(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets the size of the bitmap in resolution independent units.
        /// </summary>
        public SizeF Size
        {
            [LocalsInit(false)]
            get => GetSize();
        }

        /// <summary>
        /// Gets the size of the bitmap in resolution dependent units, (pixels).
        /// </summary>
        public SizeU PixelSize
        {
            [LocalsInit(false)]
            get => GetPixelSize();
        }

        /// <summary>
        /// Retrieve the format of the bitmap.
        /// </summary>
        public D2D1PixelFormat PixelFormat
        {
            [LocalsInit(false)]
            get => GetPixelFormat();
        }

        /// <summary>
        /// Gets the DPI of the bitmap.
        /// </summary>
        public PointF Dpi
        {
            [LocalsInit(false)]
            get => GetDpi();
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private SizeF GetSize()
        {
            SizeF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSize);
            ((delegate* unmanaged[Stdcall]<void*, SizeF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private SizeU GetPixelSize()
        {
            SizeU result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPixelSize);
            ((delegate* unmanaged[Stdcall]<void*, SizeU*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private D2D1PixelFormat GetPixelFormat()
        {
            D2D1PixelFormat result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPixelFormat);
            ((delegate* unmanaged[Stdcall]<void*, D2D1PixelFormat*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private PointF GetDpi()
        {
            PointF result;
            GetDpiCore((float*)&result, (float*)&result + 1);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private void GetDpiCore(float* dpiX, float* dpiY)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDpi);
            ((delegate* unmanaged[Stdcall]<void*, float*, float*, void>)functionPointer)(nativePointer, dpiX, dpiY);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void CopyFromBitmap(D2D1Bitmap bitmap)
            => CopyFromBitmap(null, bitmap, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFromBitmap(in PointU destPoint, D2D1Bitmap bitmap, in RectU srcRect)
            => CopyFromBitmap(UnsafeHelper.AsPointerIn(in destPoint), bitmap, UnsafeHelper.AsPointerIn(in srcRect));

        public void CopyFromBitmap(PointU* destPoint, D2D1Bitmap bitmap, RectU* srcRect)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CopyFromBitmap);
            int hr = ((delegate* unmanaged[Stdcall]<void*, PointU*, void*, RectU*, int>)functionPointer)(nativePointer, destPoint, bitmap.NativePointer, srcRect);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void CopyFromRenderTarget(D2D1RenderTarget renderTarget)
            => CopyFromRenderTarget(null, renderTarget, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFromRenderTarget(in PointU destPoint, D2D1RenderTarget renderTarget, in RectU srcRect)
            => CopyFromRenderTarget(UnsafeHelper.AsPointerIn(in destPoint), renderTarget, UnsafeHelper.AsPointerIn(in srcRect));

        public void CopyFromRenderTarget(PointU* destPoint, D2D1RenderTarget renderTarget, RectU* srcRect)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CopyFromRenderTarget);
            int hr = ((delegate* unmanaged[Stdcall]<void*, PointU*, void*, RectU*, int>)functionPointer)(nativePointer, destPoint, renderTarget.NativePointer, srcRect);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void CopyFromMemory(void* srcData, uint pitch)
            => CopyFromMemory(null, srcData, pitch);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFromMemory(in RectU dstRect, void* srcData, uint pitch)
            => CopyFromMemory(UnsafeHelper.AsPointerIn(in dstRect), srcData, pitch);

        public void CopyFromMemory(RectU* dstRect, void* srcData, uint pitch)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CopyFromMemory);
            int hr = ((delegate* unmanaged[Stdcall]<void*, RectU*, void*, uint, int>)functionPointer)(nativePointer, dstRect, srcData, pitch);
            ThrowHelper.ThrowExceptionForHR(hr);
        }
    }
}