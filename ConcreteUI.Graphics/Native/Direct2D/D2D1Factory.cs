using System;
using System.Runtime.CompilerServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;
using WitherTorch.Common.Windows.ObjectModels;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class D2D1Factory : ComObject
    {
        public static readonly Guid IID_IDXGIFactory = new Guid(0x06152247, 0x6f50, 0x465a, 0x92, 0x45, 0x11, 0x8b, 0xfd, 0x3b, 0x60, 0x07);

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            ReloadSystemMetrics = _Start,
            GetDesktopDpi,
            CreateRectangleGeometry,
            CreateRoundedRectangleGeometry,
            CreateEllipseGeometry,
            CreateGeometryGroup,
            CreateTransformedGeometry,
            CreatePathGeometry,
            CreateStrokeStyle,
            CreateDrawingStateBlock,
            CreateWicBitmapRenderTarget,
            CreateHwndRenderTarget,
            CreateDxgiSurfaceRenderTarget,
            CreateDCRenderTarget,
            _End
        }

        public D2D1Factory() : base() { }

        public D2D1Factory(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static D2D1Factory Create(D2D1FactoryType factoryType)
            => Create(factoryType, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static D2D1Factory Create(D2D1FactoryType factoryType, in D2D1FactoryOptions options)
            => Create(factoryType, UnsafeHelper.AsPointerIn(in options));

        [LocalsInit(false)]
        public static D2D1Factory Create(D2D1FactoryType factoryType, D2D1FactoryOptions* options)
        {
            void* nativePointer;
            Guid guid = IID_IDXGIFactory;
            int hr = D2D1.D2D1CreateFactory(factoryType, &guid, options, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1Factory(nativePointer, ReferenceType.Owned);
        }

        public D2D1RectangleGeometry CreateRectangleGeometry(in RectF rect)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateRectangleGeometry);
            int hr = ((delegate* unmanaged[Stdcall]<void*, RectF*, void**, int>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(rect), &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1RectangleGeometry(nativePointer, ReferenceType.Owned);
        }

        public D2D1RoundedRectangleGeometry CreateRoundedRectangleGeometry(in D2D1RoundedRectangle roundedRect)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateRoundedRectangleGeometry);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1RoundedRectangle*, void**, int>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(roundedRect), &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1RoundedRectangleGeometry(nativePointer, ReferenceType.Owned);
        }

        public D2D1EllipseGeometry CreateEllipseGeometry(in D2D1Ellipse ellipse)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateEllipseGeometry);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1Ellipse*, void**, int>)functionPointer)(nativePointer, UnsafeHelper.AsPointerIn(ellipse), &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1EllipseGeometry(nativePointer, ReferenceType.Owned);
        }

        /// <summary>
        /// Returns an initially empty path geometry interface. A geometry sink is created
        /// off the interface to populate it.
        /// </summary>
        public D2D1PathGeometry CreatePathGeometry()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreatePathGeometry);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1PathGeometry(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateStrokeStyle(D2D1StrokeStyleProperties*, float*, uint)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1StrokeStyle CreateStrokeStyle(in D2D1StrokeStyleProperties strokeStyleProperties)
            => CreateStrokeStyle(UnsafeHelper.AsPointerIn(in strokeStyleProperties), null, 0u);

        /// <inheritdoc cref="CreateStrokeStyle(D2D1StrokeStyleProperties*, float*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1StrokeStyle CreateStrokeStyle(in D2D1StrokeStyleProperties strokeStyleProperties, params float[] dashes)
        {
            fixed (float* ptr = dashes)
                return CreateStrokeStyle(strokeStyleProperties, ptr, unchecked((uint)dashes.Length));
        }

        /// <inheritdoc cref="CreateStrokeStyle(D2D1StrokeStyleProperties*, float*, uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1StrokeStyle CreateStrokeStyle(in D2D1StrokeStyleProperties strokeStyleProperties, float* dashes, uint dashesCount)
            => CreateStrokeStyle(UnsafeHelper.AsPointerIn(in strokeStyleProperties), dashes, dashesCount);

        /// <summary>
        /// Allows a non-default stroke style to be specified for a given geometry at draw time.
        /// </summary>
        public D2D1StrokeStyle CreateStrokeStyle(D2D1StrokeStyleProperties* strokeStyleProperties, float* dashes, uint dashesCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateStrokeStyle);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1StrokeStyleProperties*, float*, uint, void**, int>)functionPointer)(nativePointer,
                strokeStyleProperties, dashes, dashesCount, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1StrokeStyle(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateHwndRenderTarget(D2D1RenderTargetProperties*, D2D1HwndRenderTargetProperties*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1HwndRenderTarget CreateHwndRenderTarget(in D2D1RenderTargetProperties renderTargetProperties,
            in D2D1HwndRenderTargetProperties hwndRenderTargetProperties)
            => CreateHwndRenderTarget(UnsafeHelper.AsPointerIn(in renderTargetProperties), UnsafeHelper.AsPointerIn(in hwndRenderTargetProperties));

        /// <summary>
        /// Creates a render target that appears on the display.
        /// </summary>
        public D2D1HwndRenderTarget CreateHwndRenderTarget(D2D1RenderTargetProperties* renderTargetProperties,
            D2D1HwndRenderTargetProperties* hwndRenderTargetProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateHwndRenderTarget);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1RenderTargetProperties*, D2D1HwndRenderTargetProperties*, void**, int>)functionPointer)(nativePointer,
                renderTargetProperties, hwndRenderTargetProperties, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1HwndRenderTarget(nativePointer, ReferenceType.Owned);
        }
    }
}