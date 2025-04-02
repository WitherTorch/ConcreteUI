using System;

using InlineMethod;

using WitherTorch.CrossNative;

namespace ConcreteUI.Graphics.Native.Direct2D.Effects
{
    public unsafe sealed class D2D1GaussianBlur : D2D1Effect //No method table
    {
        public static readonly Guid CLSID_D2D1GaussianBlur = new Guid(0x1feb6d69, 0x2fe6, 0x4ac9, 0x8c, 0x58, 0x1d, 0x7f, 0x93, 0xe7, 0xa6, 0xa5);

        public D2D1GaussianBlur() : base() { }

        public D2D1GaussianBlur(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public float StandardDeviation
        {
            get => GetValue<float>(D2D1GaussianBlurProperty.StandardDeviation);
            set => SetValue(D2D1GaussianBlurProperty.StandardDeviation, value);
        }

        public D2D1GaussianBlurOptimization Optimization
        {
            get => GetValue<D2D1GaussianBlurOptimization>(D2D1GaussianBlurProperty.Optimization);
            set => SetValue(D2D1GaussianBlurProperty.Optimization, value);
        }

        public D2D1BorderMode BorderMode
        {
            get => GetValue<D2D1BorderMode>(D2D1GaussianBlurProperty.BorderMode);
            set => SetValue(D2D1GaussianBlurProperty.BorderMode, value);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public T GetValue<T>(D2D1GaussianBlurProperty property) where T : unmanaged
        {
            return GetValue<T>((uint)property);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public void SetValue<T>(D2D1GaussianBlurProperty property, T value) where T : unmanaged
        {
            SetValue((uint)property, value);
        }
    }
}