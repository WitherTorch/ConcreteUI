using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct2D.Effects;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// The device context represents a set of state and a command buffer that is used
    /// to render to a target bitmap.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe class D2D1DeviceContext : D2D1RenderTarget
    {
        protected new enum MethodTable
        {
            _Start = D2D1RenderTarget.MethodTable._End,
            CreateBitmap = _Start,
            CreateBitmapFromWicBitmap,
            CreateColorContext,
            CreateColorContextFromFilename,
            CreateColorContextFromWicColorContext,
            CreateBitmapFromDxgiSurface,
            CreateEffect,
            CreateGradientStopCollection,
            CreateImageBrush,
            CreateBitmapBrush,
            CreateCommandList,
            IsDxgiFormatSupported,
            IsBufferPrecisionSupported,
            GetImageLocalBounds,
            GetImageWorldBounds,
            GetGlyphRunWorldBounds,
            GetDevice,
            SetTarget,
            GetTarget,
            SetRenderingControls,
            GetRenderingControls,
            SetPrimitiveBlend,
            GetPrimitiveBlend,
            SetUnitMode,
            GetUnitMode,
            DrawGlyphRun,
            DrawImage,
            DrawGdiMetafile,
            DrawBitmap,
            PushLayer,
            InvalidateEffectInputRectangle,
            GetEffectInvalidRectangleCount,
            GetEffectInvalidRectangles,
            GetEffectRequiredInputRectangles,
            FillOpacityMask,
            _End
        }

        public D2D1DeviceContext() : base() { }

        public D2D1DeviceContext(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets or sets the target that this device context is currently pointing to. <br/>
        /// The image can be a command list or a bitmap created with the <see cref="D2D1BitmapOptions.Target"/> flag.
        /// </summary>
        public D2D1Image Target
        {
            [LocalsInit(false)]
            get => GetTarget();
            set => SetTarget(value);
        }

        /// <summary>
        /// Creates a bitmap with extended bitmap properties.
        /// </summary>
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1Bitmap1 CreateBitmap1(SizeU size, in D2D1BitmapProperties1 bitmapProperties)
            => CreateBitmap1(size, null, 0u, bitmapProperties);

        /// <inheritdoc cref="CreateBitmap1(SizeU, void*, uint, D2D1BitmapProperties1*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Bitmap1 CreateBitmap1(SizeU size, void* sourceData, uint pitch, in D2D1BitmapProperties1 bitmapProperties)
            => CreateBitmap1(size, sourceData, pitch, UnsafeHelper.AsPointerIn(in bitmapProperties));

        /// <summary>
        /// Creates a bitmap with extended bitmap properties, potentially from a block of memory.
        /// </summary>
        public D2D1Bitmap1 CreateBitmap1(SizeU size, void* sourceData, uint pitch, D2D1BitmapProperties1* bitmapProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmap);
            int hr = ((delegate*<void*, SizeU, void*, uint, D2D1BitmapProperties1*, void**, int>)functionPointer)(nativePointer,
                size, sourceData, pitch, bitmapProperties, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1Bitmap1(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Creates a bitmap from a DXGI surface.
        /// </summary>
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1Bitmap1 CreateBitmapFromDxgiSurface(DXGISurface surface) => CreateBitmapFromDxgiSurface(surface, null);

        /// <inheritdoc cref="CreateBitmapFromDxgiSurface(DXGISurface, D2D1BitmapProperties1*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Bitmap1 CreateBitmapFromDxgiSurface(DXGISurface surface, in D2D1BitmapProperties1 bitmapProperties)
            => CreateBitmapFromDxgiSurface(surface, UnsafeHelper.AsPointerIn(in bitmapProperties));

        /// <summary>
        /// Creates a bitmap from a DXGI surface with a set of extended properties.
        /// </summary>
        public D2D1Bitmap1 CreateBitmapFromDxgiSurface(DXGISurface surface, D2D1BitmapProperties1* bitmapProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapFromDxgiSurface);
            int hr = ((delegate*<void*, void*, D2D1BitmapProperties1*, void**, int>)functionPointer)(nativePointer,
                surface.NativePointer, bitmapProperties, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1Bitmap1(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <inheritdoc cref="CreateEffect(Guid*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Effect CreateEffect(in Guid effectId)
            => CreateEffect(UnsafeHelper.AsPointerIn(in effectId));

        /// <summary>
        /// Create a new effect, the effect must either be built in or previously registered
        /// through ID2D1Factory1::RegisterEffectFromStream or ID2D1Factory1::RegisterEffectFromString.
        /// </summary>
        public D2D1Effect CreateEffect(Guid* effectId)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateEffect);
            int hr = ((delegate*<void*, Guid*, void**, int>)functionPointer)(nativePointer, effectId, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1Effect(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        /// <summary>
        /// Indicates whether the format is supported by D2D.
        /// </summary>
        public bool IsDxgiFormatSupported(DXGIFormat format)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.IsDxgiFormatSupported);
            return ((delegate*<void*, DXGIFormat, bool>)functionPointer)(nativePointer, format);
        }

        /// <summary>
        /// Retrieves the device associated with this device context.
        /// </summary>
        public D2D1Device GetDevice()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDevice);
            ((delegate*<void*, void**, void>)functionPointer)(nativePointer, &nativePointer);
            return nativePointer == null ? null : new D2D1Device(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetTarget(D2D1Image image)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetTarget);
            ((delegate*<void*, void*, void>)functionPointer)(nativePointer, image == null ? null : image.NativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1Image GetTarget()
        {
            void* image;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetTarget);
            ((delegate*<void*, void**, void>)functionPointer)(nativePointer, &image);
            return image == null ? null : new D2D1Image(image, ReferenceType.Owned);
        }

        /// <inheritdoc cref="DrawImage(D2D1Image, PointF*, RectF*, D2D1InterpolationMode, D2D1CompositeMode)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void DrawImage(D2D1Image image,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => DrawImage(image, null, null, interpolationMode, compositeMode);

        /// <inheritdoc cref="DrawImage(D2D1Image, PointF*, RectF*, D2D1InterpolationMode, D2D1CompositeMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(D2D1Image image, in PointF targetOffset,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => DrawImage(image, UnsafeHelper.AsPointerIn(in targetOffset), null, interpolationMode, compositeMode);

        /// <inheritdoc cref="DrawImage(D2D1Image, PointF*, RectF*, D2D1InterpolationMode, D2D1CompositeMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(D2D1Image image, in PointF targetOffset, in RectF imageRectangle,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
            => DrawImage(image, UnsafeHelper.AsPointerIn(in targetOffset), UnsafeHelper.AsPointerIn(in imageRectangle), interpolationMode, compositeMode);

        /// <summary>
        /// Draw an image to the device context. The image represents either a concrete
        /// bitmap or the output of an effect graph.
        /// </summary>
        public void DrawImage(D2D1Image image, PointF* targetOffset, RectF* imageRectangle,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, D2D1CompositeMode compositeMode = D2D1CompositeMode.SourceOver)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawImage);
            ((delegate*<void*, void*, PointF*, RectF*, D2D1InterpolationMode, D2D1CompositeMode, void>)functionPointer)(nativePointer,
                image.NativePointer, targetOffset, imageRectangle, interpolationMode, compositeMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity = 1.0f,
           D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear)
            => DrawBitmap(bitmap, UnsafeHelper.AsPointerIn(in destinationRectangle), opacity, interpolationMode, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity = 1.0f,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear)
            => DrawBitmap(bitmap, UnsafeHelper.AsPointerIn(in destinationRectangle), opacity, interpolationMode,
                UnsafeHelper.AsPointerIn(in sourceRectangle), null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, in Matrix4x4 perspectiveTransform, float opacity = 1.0f,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear)
            => DrawBitmap(bitmap, UnsafeHelper.AsPointerIn(in destinationRectangle), opacity, interpolationMode,
                UnsafeHelper.AsPointerIn(in sourceRectangle), UnsafeHelper.AsPointerIn(in perspectiveTransform));

        public void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1.0f,
            D2D1InterpolationMode interpolationMode = D2D1InterpolationMode.Linear, RectF* sourceRectangle = null, Matrix4x4* perspectiveTransform = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawBitmap);
            ((delegate*<void*, void*, RectF*, float, D2D1InterpolationMode, RectF*, Matrix4x4*, void>)functionPointer)(nativePointer,
                bitmap.NativePointer, destinationRectangle, opacity, interpolationMode, sourceRectangle, perspectiveTransform);
        }
    }
}