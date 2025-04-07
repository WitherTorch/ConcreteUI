using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D;
using System.Collections.Generic;
using WitherTorch.Common.Buffers;
using System.Drawing;
using System.Runtime.Remoting.Contexts;

namespace ConcreteUI.Theme
{
    partial class ThemedBrushFactory
    {
        private sealed class AmplifiedThemedBrushFactory : IThemedBrushFactory
        {
            private readonly IThemedBrushFactory _original;
            private readonly float _amplifier;

            public AmplifiedThemedBrushFactory(IThemedBrushFactory original, float amplifier)
            {
                _original = original;
                _amplifier = amplifier;
            }

            public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context)
                => GetBrushAmplified(context, _original.CreateDefaultBrush(context), _amplifier);

            public unsafe D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material)
                => GetBrushAmplified(context, _original.CreateBrushByMaterial(context, material), _amplifier);

            public IEnumerable<WindowMaterial> GetVariants() => _original.GetVariants();

            private static D2D1ColorF GetColorAmplified(in D2D1ColorF original, float amplifier)
            {
                float originalR = original.R * 255.0f;
                float originalG = original.G * 255.0f;
                float originalB = original.B * 255.0f;
                float r, g, b;
                if ((r = originalR * amplifier) > 255)
                {
                    amplifier = (255.0f / originalR) - 0.0001f;
                    return GetColorAmplified(original, amplifier);
                }
                if ((g = originalG * amplifier) > 255)
                {
                    amplifier = (255.0f / originalG) - 0.0001f;
                    return GetColorAmplified(original, amplifier);
                }
                if ((b = originalB * amplifier) > 255)
                {
                    amplifier = (255.0f / originalB) - 0.0001f;
                    return GetColorAmplified(original, amplifier);
                }
                return new D2D1ColorF(r / 255f, g / 255f, b / 255f, original.A);
            }

            private static unsafe D2D1Brush GetBrushAmplified(D2D1DeviceContext context, D2D1Brush brush, float amplifier)
            {
                switch (brush)
                {
                    case D2D1SolidColorBrush castedBrush:
                        castedBrush.Color = GetColorAmplified(castedBrush.Color, amplifier);
                        break;
                    case D2D1LinearGradientBrush castedBrush:
                        {
                            D2D1GradientStopCollection collection = castedBrush.GradientStopCollection;
                            uint count = collection.Count;
                            if (count == 0)
                                break;
                            ArrayPool<D2D1GradientStop> pool = ArrayPool<D2D1GradientStop>.Shared;
                            D2D1GradientStop[] stops = pool.Rent(count);
                            for (uint i = 0; i < count; i++)
                            {
                                D2D1GradientStop stop = collection[i];
                                stop.Color = GetColorAmplified(stop.Color, amplifier);
                                stops[i++] = stop;
                            }
                            D2D1GradientStopCollection newCollection;
                            fixed (D2D1GradientStop* ptr = stops)
                                newCollection = context.CreateGradientStopCollection(stops, collection.ColorInterpolationGamma, collection.ExtendMode);
                            collection.Dispose();
                            pool.Return(stops);
                            brush = context.CreateLinearGradientBrush(
                                new D2D1LinearGradientBrushProperties(castedBrush.StartPoint, castedBrush.EndPoint),
                                new D2D1BrushProperties(castedBrush.Opacity, castedBrush.Transform), newCollection);
                            castedBrush.Dispose();
                        }
                        break;
                    case D2D1RadialGradientBrush castedBrush:
                        {
                            D2D1GradientStopCollection collection = castedBrush.GradientStopCollection;
                            uint count = collection.Count;
                            if (count == 0)
                                break;
                            ArrayPool<D2D1GradientStop> pool = ArrayPool<D2D1GradientStop>.Shared;
                            D2D1GradientStop[] stops = pool.Rent(count);
                            for (uint i = 0; i < count; i++)
                            {
                                D2D1GradientStop stop = collection[i];
                                stop.Color = GetColorAmplified(stop.Color, amplifier);
                                stops[i++] = stop;
                            }
                            D2D1GradientStopCollection newCollection;
                            fixed (D2D1GradientStop* ptr = stops)
                                newCollection = context.CreateGradientStopCollection(stops, collection.ColorInterpolationGamma, collection.ExtendMode);
                            collection.Dispose();
                            pool.Return(stops);
                            brush = context.CreateRadialGradientBrush(
                                new D2D1RadialGradientBrushProperties(castedBrush.Center, castedBrush.GradientOriginOffset, castedBrush.RadiusX, castedBrush.RadiusY),
                                new D2D1BrushProperties(castedBrush.Opacity, castedBrush.Transform), newCollection);
                            castedBrush.Dispose();
                        }
                        break;
                }
                return brush;
            }
        }
    }
}
