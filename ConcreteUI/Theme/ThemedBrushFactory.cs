using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Utils;

namespace ConcreteUI.Theme
{
    public interface IThemedBrushFactory
    {
        D2D1Brush CreateDefaultBrush(D2D1DeviceContext context);

        D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material);

        IEnumerable<WindowMaterial> GetVariants();
    }

    public static partial class ThemedBrushFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedBrushFactory FromColor(in D2D1ColorF color) => CreateBuilder(color).Build();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedBrushFactory FromColorFactory(IThemedColorFactory factory) 
            => new ThemedColorFactoryAdapter(factory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedBrushFactory AmplifiedFrom(IThemedBrushFactory factory, float amplifier) 
            => new AmplifiedThemedBrushFactory(factory, amplifier);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemedBrushFactory FromFunction(Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> function) 
            => new FunctionThemedBrushFactoryImpl(function);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Builder CreateBuilder(in D2D1ColorF baseBrushColor) => new Builder(CreateFactoryByColor(baseBrushColor));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Builder CreateBuilder(Func<D2D1DeviceContext, D2D1Brush> baseBrushFactory) => new Builder(baseBrushFactory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Builder CreateBuilder(IThemedBrushFactory originalFactory)
        {
            switch (originalFactory)
            {
                case SimpleThemedBrushFactoryImpl factory:
                    return new Builder(factory.Destruct());
                case ThemedBrushFactoryImpl factory:
                    return new Builder(factory.Destruct(out byte[] variantKeys, out Func<D2D1DeviceContext, D2D1Brush>[] variantBrushFactories), variantKeys, variantBrushFactories);
                default:
                    Builder builder = new Builder(originalFactory.CreateDefaultBrush);
                    foreach (WindowMaterial variant in originalFactory.GetVariants())
                        builder = builder.WithVariant(variant, context => originalFactory.CreateBrushByMaterial(context, variant));
                    return builder;
            }
        }

        private static Func<D2D1DeviceContext, D2D1Brush> CreateFactoryByColor(D2D1ColorF color)
            => context => context.CreateSolidColorBrush(color);
    }
}
