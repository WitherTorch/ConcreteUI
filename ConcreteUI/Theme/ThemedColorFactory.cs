using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Utils;

namespace ConcreteUI.Theme
{
    public interface IThemedColorFactory
    {
        D2D1ColorF CreateDefaultColor();

        D2D1ColorF CreateColorByMaterial(WindowMaterial material);

        IEnumerable<WindowMaterial> GetVariants();
    }

    public static partial class ThemedColorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedColorFactory FromColor(in D2D1ColorF color)
            => new SimpleThemedColorFactoryImpl(color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedColorFactory FromFunction(Func<WindowMaterial, D2D1ColorF> function)
            => new FunctionThemedColorFactoryImpl(function);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Builder CreateBuilder(in D2D1ColorF baseColor) => new Builder(baseColor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Builder CreateBuilder(IThemedColorFactory originalFactory)
        {
            switch (originalFactory)
            {
                case SimpleThemedColorFactoryImpl factory:
                    return new Builder(factory.Destruct());
                case ThemedColorFactoryImpl factory:
                    return new Builder(factory.Destruct(out byte[] variantKeys, out D2D1ColorF[] variantColors), variantKeys, variantColors);
                default:
                    D2D1ColorF baseColor = originalFactory.CreateDefaultColor();
                    Builder builder = new Builder(baseColor);
                    foreach (WindowMaterial variant in originalFactory.GetVariants())
                        builder = builder.WithVariant(variant, originalFactory.CreateColorByMaterial(variant));
                    return builder;
            }
        }
    }
}
