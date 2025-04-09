using System.Runtime.CompilerServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct2D.Geometry;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class D2D1Factory : ComObject
    {
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
    }
}