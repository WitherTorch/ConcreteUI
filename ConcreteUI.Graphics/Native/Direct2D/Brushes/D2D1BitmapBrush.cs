using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D.Brushes
{
    /// <summary>
    /// A bitmap brush allows a bitmap to be used to fill a geometry.
    /// </summary>
    public unsafe sealed class D2D1BitmapBrush : D2D1Brush
    {
        private new enum MethodTable
        {
            _Start = D2D1Brush.MethodTable._End,
            SetExtendModeX = _Start,
            SetExtendModeY,
            SetInterpolationMode,
            SetBitmap,
            GetExtendModeX,
            GetExtendModeY,
            GetInterpolationMode,
            GetBitmap,
            _End
        }

        public D2D1BitmapBrush(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        /// <summary>
        /// Gets or sets how the bitmap is to be treated outside of its natural extent on the X
        /// axis.
        /// </summary>
        public D2D1ExtendMode ExtendModeX
        {
            get => GetExtendModeX();
            set => SetExtendModeX(value);
        }


        /// <summary>
        /// Gets or sets how the bitmap is to be treated outside of its natural extent on the Y
        /// axis.
        /// </summary>
        public D2D1ExtendMode ExtendModeY
        {
            get => GetExtendModeY();
            set => SetExtendModeY(value);
        }

        /// <summary>
        /// Gets or sets the interpolation mode used when this brush is used.
        /// </summary>
        public D2D1BitmapInterpolationMode InterpolationMode
        {
            get => GetInterpolationMode();
            set => SetInterpolationMode(value);
        }

        /// <summary>
        /// Gets or sets the bitmap associated as the source of this brush.
        /// </summary>
        public D2D1Bitmap Bitmap
        {
            get => GetBitmap();
            set => SetBitmap(value);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetExtendModeX(D2D1ExtendMode extendMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetExtendModeX);
            ((delegate*<void*, D2D1ExtendMode, void>)functionPointer)(nativePointer, extendMode);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetExtendModeY(D2D1ExtendMode extendMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetExtendModeY);
            ((delegate*<void*, D2D1ExtendMode, void>)functionPointer)(nativePointer, extendMode);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetInterpolationMode(D2D1BitmapInterpolationMode interpolationMode)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetInterpolationMode);
            ((delegate*<void*, D2D1BitmapInterpolationMode, void>)functionPointer)(nativePointer, interpolationMode);
        }

        [Inline(InlineBehavior.Remove)]
        private void SetBitmap(D2D1Bitmap bitmap)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetBitmap);
            ((delegate*<void*, void*, void>)functionPointer)(nativePointer, bitmap == null ? null : bitmap.NativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1ExtendMode GetExtendModeX()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetExtendModeX);
            return ((delegate*<void*, D2D1ExtendMode>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1ExtendMode GetExtendModeY()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetExtendModeY);
            return ((delegate*<void*, D2D1ExtendMode>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1BitmapInterpolationMode GetInterpolationMode()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInterpolationMode);
            return ((delegate*<void*, D2D1BitmapInterpolationMode>)functionPointer)(nativePointer);
        }

        private D2D1Bitmap GetBitmap()
        {
            void* pBitmap;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBitmap);
            ((delegate*<void*, void**, void>)functionPointer)(nativePointer, &pBitmap);
            return pBitmap == null ? null : new D2D1Bitmap(pBitmap, ReferenceType.Owned);
        }
    }
}
