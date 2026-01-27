using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;
using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Graphics.Native.WIC;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Structures;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Represents an object that can receive drawing commands.
    /// </summary>
    /// <remarks>
    /// Classes that inherit from <see cref="D2D1RenderTarget"/> render the drawing commands they receive in different
    /// ways.
    /// </remarks>
    [SuppressUnmanagedCodeSecurity]
    public unsafe class D2D1RenderTarget : D2D1Resource
    {
        protected new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            CreateBitmap = _Start,
            CreateBitmapFromWicBitmap,
            CreateSharedBitmap,
            CreateBitmapBrush,
            CreateSolidColorBrush,
            CreateGradientStopCollection,
            CreateLinearGradientBrush,
            CreateRadialGradientBrush,
            CreateCompatibleRenderTarget,
            CreateLayer,
            CreateMesh,
            DrawLine,
            DrawRectangle,
            FillRectangle,
            DrawRoundedRectangle,
            FillRoundedRectangle,
            DrawEllipse,
            FillEllipse,
            DrawGeometry,
            FillGeometry,
            FillMesh,
            FillOpacityMask,
            DrawBitmap,
            DrawText,
            DrawTextLayout,
            DrawGlyphRun,
            SetTransform,
            GetTransform,
            SetAntialiasMode,
            GetAntialiasMode,
            SetTextAntialiasMode,
            GetTextAntialiasMode,
            SetTextRenderingParams,
            GetTextRenderingParams,
            SetTags,
            GetTags,
            PushLayer,
            PopLayer,
            Flush,
            SaveDrawingState,
            RestoreDrawingState,
            PushAxisAlignedClip,
            PopAxisAlignedClip,
            Clear,
            BeginDraw,
            EndDraw,
            GetPixelFormat,
            SetDpi,
            GetDpi,
            GetSize,
            GetPixelSize,
            GetMaximumBitmapSize,
            IsSupported,
            _End
        }

        public D2D1RenderTarget() : base() { }

        public D2D1RenderTarget(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public Matrix3x2 Transform
        {
            get => GetTransform();
            [LocalsInit(false)]
            set => SetTransform(value);
        }

        public D2D1AntialiasMode AntialiasMode
        {
            get => GetAntialiasMode();
            set => SetAntialiasMode(value);
        }

        public D2D1TextAntialiasMode TextAntialiasMode
        {
            get => GetTextAntialiasMode();
            set => SetTextAntialiasMode(value);
        }

        /// <summary>
        /// Gets or sets the DPI on the render target. 
        /// </summary>
        /// <remarks>
        /// This results in the render target being interpreted to a different scale. Neither DPI can be negative. <br/>
        /// If zero is specified for both, the system DPI is chosen. If one is zero and the other unspecified, the DPI is not changed.
        /// </remarks>
        public PointF Dpi
        {
            get => GetDpi();
            [LocalsInit(false)]
            set => SetDpi(value);
        }

        public D2D1PixelFormat PixelFormat
        {
            [LocalsInit(false)]
            get => GetPixelFormat();
        }

        /// <summary>
        /// Gets the size of the render target in DIPs.
        /// </summary>
        public SizeF Size
        {
            [LocalsInit(false)]
            get => GetSize();
        }

        /// <summary>
        /// Returns the size of the render target in pixels.
        /// </summary>
        public SizeU PixelSize
        {
            [LocalsInit(false)]
            get => GetPixelSize();
        }

        /// <summary>
        /// Create an uninitialized D2D bitmap.
        /// </summary>
        public D2D1Bitmap CreateBitmap(SizeU size, in D2D1BitmapProperties bitmapProperties)
            => CreateBitmap(size, null, 0u, bitmapProperties);

        /// <inheritdoc cref="CreateBitmap(SizeU, void*, uint, D2D1BitmapProperties*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Bitmap CreateBitmap(SizeU size, void* srcData, uint pitch, in D2D1BitmapProperties bitmapProperties)
            => CreateBitmap(size, srcData, pitch, UnsafeHelper.AsPointerIn(in bitmapProperties));

        /// <summary>
        /// Create a D2D bitmap by copying from memory, or create uninitialized.
        /// </summary>
        public D2D1Bitmap CreateBitmap(SizeU size, void* srcData, uint pitch, D2D1BitmapProperties* bitmapProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmap);
            int hr = ((delegate* unmanaged[Stdcall]<void*, SizeU, void*, uint, D2D1BitmapProperties*, void**, int>)functionPointer)(nativePointer,
                size, srcData, pitch, bitmapProperties, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1Bitmap(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateBitmapFromWicBitmap(WICBitmapSource, D2D1BitmapProperties*)" />
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1Bitmap CreateBitmapFromWicBitmap(WICBitmapSource wicBitmapSource) => CreateBitmapFromWicBitmap(wicBitmapSource, null);

        /// <inheritdoc cref="CreateBitmapFromWicBitmap(WICBitmapSource, D2D1BitmapProperties*)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Bitmap CreateBitmapFromWicBitmap(WICBitmapSource wicBitmapSource, in D2D1BitmapProperties bitmapProperties)
            => CreateBitmapFromWicBitmap(wicBitmapSource, UnsafeHelper.AsPointerIn(in bitmapProperties));

        /// <summary>
        /// Create a D2D bitmap by copying a WIC bitmap.
        /// </summary>
        public D2D1Bitmap CreateBitmapFromWicBitmap(WICBitmapSource wicBitmapSource, D2D1BitmapProperties* bitmapProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapFromWicBitmap);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, D2D1BitmapProperties*, void**, int>)functionPointer)(nativePointer,
                wicBitmapSource.NativePointer, bitmapProperties, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1Bitmap(nativePointer, ReferenceType.Owned);
        }

        /// <summary>
        /// Creates a bitmap brush. The bitmap is tiled to fill or pen a geometry.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1BitmapBrush CreateBitmapBrush(D2D1Bitmap bitmap, in D2D1BitmapBrushProperties bitmapBrushProperties)
            => CreateBitmapBrush(bitmap, UnsafeHelper.AsPointerIn(in bitmapBrushProperties), null);

        /// <summary>
        /// Creates a bitmap brush. The bitmap is scaled, rotated, skewed to fill or pen a geometry.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1BitmapBrush CreateBitmapBrush(D2D1Bitmap bitmap, in D2D1BrushProperties brushProperties)
            => CreateBitmapBrush(bitmap, null, UnsafeHelper.AsPointerIn(in brushProperties));

        /// <inheritdoc cref="CreateBitmapBrush(D2D1Bitmap, D2D1BitmapBrushProperties*, D2D1BrushProperties*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1BitmapBrush CreateBitmapBrush(D2D1Bitmap bitmap, in D2D1BitmapBrushProperties bitmapBrushProperties, in D2D1BrushProperties brushProperties)
            => CreateBitmapBrush(bitmap, UnsafeHelper.AsPointerIn(in bitmapBrushProperties), UnsafeHelper.AsPointerIn(in brushProperties));

        /// <summary>
        /// Creates a bitmap brush. The bitmap is scaled, rotated, skewed or tiled to fill or pen a geometry.
        /// </summary>
        public D2D1BitmapBrush CreateBitmapBrush(D2D1Bitmap bitmap, D2D1BitmapBrushProperties* bitmapBrushProperties, D2D1BrushProperties* brushProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapBrush);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, D2D1BitmapBrushProperties*, D2D1BrushProperties*, void**, int>)functionPointer)(nativePointer,
                bitmap == null ? null : bitmap.NativePointer, bitmapBrushProperties, brushProperties, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1BitmapBrush(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1SolidColorBrush CreateSolidColorBrush(in D2D1ColorF color)
            => CreateSolidColorBrush(UnsafeHelper.AsPointerIn(in color), null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1SolidColorBrush CreateSolidColorBrush(in D2D1ColorF color, in D2D1BrushProperties brushProperties)
            => CreateSolidColorBrush(UnsafeHelper.AsPointerIn(in color), UnsafeHelper.AsPointerIn(in brushProperties));

        public D2D1SolidColorBrush CreateSolidColorBrush(D2D1ColorF* color, D2D1BrushProperties* brushProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateSolidColorBrush);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1ColorF*, D2D1BrushProperties*, void**, int>)functionPointer)(nativePointer, color, brushProperties, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1SolidColorBrush(nativePointer, ReferenceType.Owned);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1GradientStopCollection CreateGradientStopCollection(params D2D1GradientStop[] gradientStops)
            => CreateGradientStopCollection(gradientStops, D2D1Gamma.Gamma_2_2, D2D1ExtendMode.Clamp);

        /// <inheritdoc cref="CreateGradientStopCollection(D2D1GradientStop*, uint, D2D1Gamma, D2D1ExtendMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1GradientStopCollection CreateGradientStopCollection(D2D1GradientStop[] gradientStops, D2D1Gamma colorInterpolationGamma, D2D1ExtendMode extendMode)
        {
            fixed (D2D1GradientStop* ptr = gradientStops)
                return CreateGradientStopCollection(ptr, unchecked((uint)gradientStops.Length), colorInterpolationGamma, extendMode);
        }

        /// <summary>
        /// A gradient stop collection represents a set of stops in an ideal unit length.
        /// This is the source resource for a linear gradient and radial gradient brush.
        /// </summary>
        /// <param name="colorInterpolationGamma">Specifies which space the color
        /// interpolation occurs in.</param>
        /// <param name="extendMode">Specifies how the gradient will be extended outside of
        /// the unit length.</param>
        public D2D1GradientStopCollection CreateGradientStopCollection(D2D1GradientStop* gradientStops, uint gradientStopsCount,
            D2D1Gamma colorInterpolationGamma, D2D1ExtendMode extendMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateGradientStopCollection);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1GradientStop*, uint, D2D1Gamma, D2D1ExtendMode, void**, int>)functionPointer)(nativePointer,
                gradientStops, gradientStopsCount, colorInterpolationGamma, extendMode, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1GradientStopCollection(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1LinearGradientBrush CreateLinearGradientBrush(in D2D1LinearGradientBrushProperties linearGradientBrushProperties,
            D2D1GradientStopCollection gradientStopCollection)
            => CreateLinearGradientBrush(UnsafeHelper.AsPointerIn(in linearGradientBrushProperties), null, gradientStopCollection);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1LinearGradientBrush CreateLinearGradientBrush(in D2D1LinearGradientBrushProperties linearGradientBrushProperties, in D2D1BrushProperties brushProperties,
            D2D1GradientStopCollection gradientStopCollection)
            => CreateLinearGradientBrush(UnsafeHelper.AsPointerIn(in linearGradientBrushProperties), UnsafeHelper.AsPointerIn(in brushProperties),
                gradientStopCollection);

        public D2D1LinearGradientBrush CreateLinearGradientBrush(D2D1LinearGradientBrushProperties* linearGradientBrushProperties, D2D1BrushProperties* brushProperties,
            D2D1GradientStopCollection gradientStopCollection)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateLinearGradientBrush);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1LinearGradientBrushProperties*, D2D1BrushProperties*, void*, void**, int>)functionPointer)(nativePointer,
                linearGradientBrushProperties, brushProperties, gradientStopCollection.NativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1LinearGradientBrush(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1RadialGradientBrush CreateRadialGradientBrush(in D2D1RadialGradientBrushProperties radialGradientBrushProperties,
            D2D1GradientStopCollection gradientStopCollection)
            => CreateRadialGradientBrush(UnsafeHelper.AsPointerIn(in radialGradientBrushProperties), null, gradientStopCollection);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1RadialGradientBrush CreateRadialGradientBrush(in D2D1RadialGradientBrushProperties radialGradientBrushProperties, in D2D1BrushProperties brushProperties,
            D2D1GradientStopCollection gradientStopCollection)
            => CreateRadialGradientBrush(UnsafeHelper.AsPointerIn(radialGradientBrushProperties), UnsafeHelper.AsPointerIn(in brushProperties),
                gradientStopCollection);

        public D2D1RadialGradientBrush CreateRadialGradientBrush(D2D1RadialGradientBrushProperties* radialGradientBrushProperties, D2D1BrushProperties* brushProperties,
            D2D1GradientStopCollection gradientStopCollection)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateRadialGradientBrush);
            int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1RadialGradientBrushProperties*, D2D1BrushProperties*, void*, void**, int>)functionPointer)(nativePointer,
                radialGradientBrushProperties, brushProperties, gradientStopCollection.NativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1RadialGradientBrush(nativePointer, ReferenceType.Owned);
        }

        /// <inheritdoc cref="CreateLayer(SizeF*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1Layer CreateLayer() => CreateLayer(null);

        /// <inheritdoc cref="CreateLayer(SizeF*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Layer CreateLayer(SizeF size) => CreateLayer(&size);

        /// <summary>
        /// Creates a layer resource that can be used on any target and which will resize
        /// under the covers if necessary.
        /// </summary>
        /// <param name="size">
        /// The resolution independent minimum size hint for the layer resource. <br/>
        /// Specify this to prevent unwanted reallocation of the layer backing store. <br/>
        /// The size is in DIPs, but, it is unaffected by the current world transform. <br/>
        /// If the size is unspecified, the returned resource is a placeholder and
        /// the backing store will be allocated to be the minimum size that can hold the content when the layer is pushed.
        /// </param>
        public D2D1Layer CreateLayer(SizeF* size)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateLayer);
            int hr = ((delegate* unmanaged[Stdcall]<void*, SizeF*, void**, int>)functionPointer)(nativePointer, size, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1Layer(nativePointer, ReferenceType.Owned);
        }

        /// <summary>
        /// Create a D2D mesh.
        /// </summary>
        public D2D1Mesh CreateMesh()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateMesh);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new D2D1Mesh(nativePointer, ReferenceType.Owned);
        }

        public void DrawLine(PointF point0, PointF point1, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawLine);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, PointF, PointF, void*, float, void*, void>)functionPointer)(nativePointer, point0, point1, brush.NativePointer,
                strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangle(in RectF rect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => DrawRectangle(UnsafeHelper.AsPointerIn(in rect), brush, strokeWidth, strokeStyle);

        public void DrawRectangle(RectF* rect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawRectangle);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, RectF*, void*, float, void*, void>)functionPointer)(nativePointer, rect, brush.NativePointer,
                strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRectangle(in RectF rect, D2D1Brush brush)
            => FillRectangle(UnsafeHelper.AsPointerIn(in rect), brush);

        public void FillRectangle(RectF* rect, D2D1Brush brush)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillRectangle);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, RectF*, void*, void>)functionPointer)(nativePointer, rect, brush.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => DrawRoundedRectangle(UnsafeHelper.AsPointerIn(in roundedRect), brush, strokeWidth, strokeStyle);

        public void DrawRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawRoundedRectangle);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1RoundedRectangle*, void*, float, void*, void>)functionPointer)(nativePointer, roundedRect, brush.NativePointer,
                strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRectangle(in D2D1RoundedRectangle roundedRect, D2D1Brush brush)
            => FillRoundedRectangle(UnsafeHelper.AsPointerIn(in roundedRect), brush);

        public void FillRoundedRectangle(D2D1RoundedRectangle* roundedRect, D2D1Brush brush)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillRoundedRectangle);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1RoundedRectangle*, void*, void>)functionPointer)(nativePointer, roundedRect, brush.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawEllipse(in D2D1Ellipse ellipse, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
            => DrawEllipse(UnsafeHelper.AsPointerIn(in ellipse), brush, strokeWidth, strokeStyle);

        public void DrawEllipse(D2D1Ellipse* ellipse, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawEllipse);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1Ellipse*, void*, float, void*, void>)functionPointer)(nativePointer, ellipse, brush.NativePointer,
                strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillEllipse(in D2D1Ellipse ellipse, D2D1Brush brush)
            => FillEllipse(UnsafeHelper.AsPointerIn(in ellipse), brush);

        public void FillEllipse(D2D1Ellipse* ellipse, D2D1Brush brush)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillEllipse);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1Ellipse*, void*, void>)functionPointer)(nativePointer, ellipse, brush.NativePointer);
        }

        public void DrawGeometry(D2D1Geometry geometry, D2D1Brush brush, float strokeWidth = 1.0f, D2D1StrokeStyle? strokeStyle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawGeometry);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, void*, void*, float, void*, void>)functionPointer)(nativePointer, geometry.NativePointer, brush.NativePointer,
                strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer);
        }

        /// <param name="opacityBrush">
        /// An optionally specified opacity brush. Only the alpha
        /// channel of the corresponding brush will be sampled and will be applied to the
        /// entire fill of the geometry. If this brush is specified, the fill brush must be
        /// a bitmap brush with an extend mode of <see cref="D2D1ExtendMode.Clamp"/>.
        /// </param>
        public void FillGeometry(D2D1Geometry geometry, D2D1Brush brush, D2D1Brush? opacityBrush = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillGeometry);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, void*, void*, void*, void>)functionPointer)(nativePointer, geometry.NativePointer, brush.NativePointer,
                opacityBrush is null ? null : opacityBrush.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, float opacity = 1.0f,
            D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear)
            => DrawBitmap(bitmap, UnsafeHelper.AsPointerIn(in destinationRectangle), opacity, interpolationMode, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBitmap(D2D1Bitmap bitmap, in RectF destinationRectangle, in RectF sourceRectangle, float opacity = 1.0f,
            D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear)
            => DrawBitmap(bitmap, UnsafeHelper.AsPointerIn(in destinationRectangle), opacity, interpolationMode, UnsafeHelper.AsPointerIn(sourceRectangle));

        public void DrawBitmap(D2D1Bitmap bitmap, RectF* destinationRectangle = null, float opacity = 1.0f,
            D2D1BitmapInterpolationMode interpolationMode = D2D1BitmapInterpolationMode.Linear, RectF* sourceRectangle = null)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawBitmap);
            ((delegate* unmanaged[Stdcall]<void*, void*, RectF*, float, D2D1BitmapInterpolationMode, RectF*, void>)functionPointer)(nativePointer, bitmap.NativePointer,
                destinationRectangle, opacity, interpolationMode, sourceRectangle);
        }

        ///<inheritdoc cref="DrawText(char*, uint, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawText(char character, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => DrawText(character, textFormat, UnsafeHelper.AsPointerIn(in layoutRect), defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="DrawText(char*, uint, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void DrawText(char character, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
        {
            DrawText(&character, 1U, textFormat, layoutRect, defaultFillBrush, options, measuringMode);
        }

        ///<inheritdoc cref="DrawText(char*, uint, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawText(string text, DWriteTextFormat textFormat, in RectF layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
            => DrawText(text, textFormat, UnsafeHelper.AsPointerIn(in layoutRect), defaultFillBrush, options, measuringMode);

        ///<inheritdoc cref="DrawText(char*, uint, DWriteTextFormat, RectF*, D2D1Brush, D2D1DrawTextOptions, DWriteMeasuringMode)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawText(string text, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
        {
            fixed (char* ptr = text)
                DrawText(ptr, unchecked((uint)text.Length), textFormat, layoutRect, defaultFillBrush, options, measuringMode);
        }

        /// <summary>
        /// Draws the text within the given layout rectangle and by default also performs
        /// baseline snapping.
        /// </summary>
        public void DrawText(char* text, uint textLength, DWriteTextFormat textFormat, RectF* layoutRect, D2D1Brush defaultFillBrush,
            D2D1DrawTextOptions options = D2D1DrawTextOptions.None, DWriteMeasuringMode measuringMode = DWriteMeasuringMode.Natural)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawText);
            ((delegate* unmanaged[Stdcall]<void*, char*, uint, void*, RectF*, void*, D2D1DrawTextOptions, DWriteMeasuringMode, void>)functionPointer)(nativePointer,
                text, textLength, textFormat.NativePointer, layoutRect, defaultFillBrush.NativePointer, options, measuringMode);
        }

        /// <summary>
        /// Draw a text layout object. <br/>
        /// If the layout is not subsequently changed, this can be more efficient than DrawText when drawing the same layout repeatedly.
        /// </summary>
        /// <param name="options">
        /// The specified text options. If <see cref="D2D1DrawTextOptions.Clip"/> is used, the text is clipped to the layout bounds. <br/>
        /// These bounds are derived from the origin and the layout bounds of the corresponding <see cref="DWriteTextLayout"/> object.
        /// </param>
        public void DrawTextLayout(PointF origin, DWriteTextLayout textLayout, D2D1Brush defaultFillBrush, D2D1DrawTextOptions options = D2D1DrawTextOptions.None)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawTextLayout);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, PointF, void*, void*, D2D1DrawTextOptions, void>)functionPointer)(nativePointer,
                origin, textLayout.NativePointer, defaultFillBrush.NativePointer, options);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetTransform(in Matrix3x2 matrix)
            => SetTransformCore(UnsafeHelper.AsPointerIn(in matrix));

        [Inline(InlineBehavior.Remove)]
        private void SetTransformCore(Matrix3x2* matrix)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetTransform);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, Matrix3x2*, void>)functionPointer)(nativePointer, matrix);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private Matrix3x2 GetTransform()
        {
            Matrix3x2 matrix;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetTransform);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, Matrix3x2*, void>)functionPointer)(nativePointer, &matrix);
            return matrix;
        }

        [Inline(InlineBehavior.Remove)]
        private void SetAntialiasMode(D2D1AntialiasMode antialiasMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetAntialiasMode);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1AntialiasMode, void>)functionPointer)(nativePointer, antialiasMode);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1AntialiasMode GetAntialiasMode()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetAntialiasMode);
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1AntialiasMode>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetTextAntialiasMode(D2D1TextAntialiasMode textAntialiasMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetTextAntialiasMode);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1TextAntialiasMode, void>)functionPointer)(nativePointer, textAntialiasMode);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1TextAntialiasMode GetTextAntialiasMode()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetTextAntialiasMode);
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1TextAntialiasMode>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="PushLayer(D2D1LayerParametersNative*, D2D1Layer?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushLayer(in D2D1LayerParameters layerParameters, D2D1Layer? layer)
            => PushLayer(layerParameters.ToNative(), layer);

        /// <inheritdoc cref="PushLayer(D2D1LayerParametersNative*, D2D1Layer?)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushLayer(in D2D1LayerParametersNative layerParameters, D2D1Layer? layer)
            => PushLayer(UnsafeHelper.AsPointerIn(in layerParameters), layer);

        /// <summary>
        /// Start a layer of drawing calls.
        /// </summary>
        /// <remarks>
        /// The way in which the layer must be resolved is specified first as well as the logical resource that stores the layer parameters. <br/>
        /// The supplied layer resource might grow if the specified content cannot fit inside it. <br/>
        /// The layer will grow monotonically on each axis. 
        /// </remarks>
        /// <param name="layerParameters">
        /// The content bounds, geometric mask, opacity, opacity mask, and antialiasing options for the layer.
        /// </param>
        /// <param name="layer">
        /// The layer that receives subsequent drawing operations. <br/>
        /// If a <see langword="null"/> is provided, then a layer resource will be allocated automatically. (Only for Windows 8 or greater)
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushLayer(D2D1LayerParametersNative* layerParameters, D2D1Layer? layer)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.PushLayer);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1LayerParametersNative*, void*, void>)functionPointer)(
                nativePointer, layerParameters, layer is null ? null : layer.NativePointer);
        }

        /// <summary>
        /// Ends a layer that was defined with particular layer resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopLayer()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.PopLayer);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, void>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void Flush() => Flush(null, null);

        [Inline(InlineBehavior.Keep, export: true)]
        public void Flush(ulong* tag1, ulong* tag2) => ThrowHelper.ThrowExceptionForHR(TryFlush(tag1, tag2));

        [Inline(InlineBehavior.Keep, export: true)]
        public int TryFlush() => TryFlush(null, null);

        public int TryFlush(ulong* tag1, ulong* tag2)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Flush);
            return ((delegate* unmanaged[Stdcall]<void*, ulong*, ulong*, int>)functionPointer)(nativePointer, tag1, tag2);
        }

        /// <inheritdoc cref="PushAxisAlignedClip(RectF*, D2D1AntialiasMode)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushAxisAlignedClip(in RectF clipRect, D2D1AntialiasMode antialiasMode)
            => PushAxisAlignedClip(UnsafeHelper.AsPointerIn(in clipRect), antialiasMode);

        /// <summary>
        /// Pushes a clip. The clip can be antialiased. The clip must be axis aligned. <br/>
        /// If the current world transform is not axis preserving, then the bounding box of the
        /// transformed clip rect will be used.
        /// </summary>
        /// <remarks>
        /// The clip will remain in effect until a PopAxisAlignedClip call is made.
        /// </remarks>
        public void PushAxisAlignedClip(RectF* clipRect, D2D1AntialiasMode antialiasMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.PushAxisAlignedClip);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, RectF*, D2D1AntialiasMode, void>)functionPointer)(nativePointer, clipRect, antialiasMode);
        }

        public void PopAxisAlignedClip()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.PopAxisAlignedClip);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, void>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void Clear() => Clear(null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(in D2D1ColorF clearColor)
            => Clear(UnsafeHelper.AsPointerIn(in clearColor));

        public void Clear(D2D1ColorF* clearColor)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Clear);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1ColorF*, void>)functionPointer)(nativePointer, clearColor);
        }

        /// <summary>
        /// Start drawing on this render target. Draw calls can only be issued between a
        /// BeginDraw and EndDraw call.
        /// </summary>
        public void BeginDraw()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.BeginDraw);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, void>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="TryEndDraw(ulong*, ulong*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void EndDraw() => EndDraw(null, null);

        /// <inheritdoc cref="TryEndDraw(ulong*, ulong*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public void EndDraw(ulong* tag1, ulong* tag2) => ThrowHelper.ThrowExceptionForHR(TryEndDraw(tag1, tag2));

        /// <inheritdoc cref="TryEndDraw(ulong*, ulong*)"/>
        [Inline(InlineBehavior.Keep, export: true)]
        public int TryEndDraw() => TryEndDraw(null, null);

        /// <summary>
        /// Ends drawing on the render target, error results can be retrieved at this time,
        /// or when calling Flush.
        /// </summary>
        public int TryEndDraw(ulong* tag1, ulong* tag2)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EndDraw);
            return ((delegate* unmanaged[Stdcall]<void*, ulong*, ulong*, int>)functionPointer)(nativePointer, tag1, tag2);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1PixelFormat GetPixelFormat()
        {
            D2D1PixelFormat format;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPixelFormat);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1PixelFormat*, void>)functionPointer)(nativePointer, &format);
            return format;
        }

        [Inline(InlineBehavior.Remove)]
        private void SetDpi(PointF dpi)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetDpi);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, float, float, void>)functionPointer)(nativePointer, dpi.X, dpi.Y);
        }

        [Inline(InlineBehavior.Remove)]
        [LocalsInit(false)]
        private PointF GetDpi()
        {
            PointF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDpi);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, float*, float*, void>)functionPointer)(nativePointer, (float*)&result, (float*)&result + 1);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private SizeF GetSize()
        {
            SizeF result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetSize);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, SizeF*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        [Inline(InlineBehavior.Remove)]
        private SizeU GetPixelSize()
        {
            SizeU result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPixelSize);
            ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, SizeU*, void>)functionPointer)(nativePointer, &result);
            return result;
        }

        /// <summary>
        /// Returns the maximum bitmap and render target size that is guaranteed to be
        /// supported by the render target.
        /// </summary>
        public uint GetMaximumBitmapSize()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMaximumBitmapSize);
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, uint>)functionPointer)(nativePointer);
        }

        /// <inheritdoc cref="IsSupported(D2D1RenderTargetProperties*)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupported(in D2D1RenderTargetProperties renderTargetProperties)
            => IsSupported(UnsafeHelper.AsPointerIn(in renderTargetProperties));

        /// <summary>
        /// Returns true if the given properties are supported by this render target. The DPI is ignored. 
        /// </summary>
        /// <remarks>
        /// NOTE: If the render target type is software, then neither <see cref="D2D1FeatureLevel.Level_9"/> 
        /// nor <see cref="D2D1FeatureLevel.Level_10"/> will be considered to be supported.
        /// </remarks>
        public bool IsSupported(D2D1RenderTargetProperties* renderTargetProperties)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.IsSupported);
            return ((delegate* unmanaged
#if NET8_0_OR_GREATER
            [Stdcall, SuppressGCTransition]
#else
            [Stdcall]
#endif
            <void*, D2D1RenderTargetProperties*, bool>)functionPointer)(nativePointer, renderTargetProperties);
        }
    }
}