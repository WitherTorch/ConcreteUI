using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{

    partial class ThemedBrushFactory
    {
        private sealed class ThemedBrushFactoryImpl : IThemedBrushFactory
        {
            private readonly Func<D2D1RenderTarget, D2D1Brush> _base;
            private readonly byte[] _variantKeys;
            private readonly Func<D2D1RenderTarget, D2D1Brush>[] _variants;

            internal ThemedBrushFactoryImpl(Func<D2D1RenderTarget, D2D1Brush> baseBrushFactory, byte[] variantKeys,
                Func<D2D1RenderTarget, D2D1Brush>[] variantBrushFactories)
            {
                if (variantKeys.Length != variantBrushFactories.Length)
                    throw new InvalidOperationException();
                Array.Sort(variantKeys, variantBrushFactories);
                _base = baseBrushFactory;
                _variantKeys = variantKeys;
                _variants = variantBrushFactories;
            }

            public D2D1Brush CreateDefaultBrush(D2D1RenderTarget renderTarget) => _base.Invoke(renderTarget);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public D2D1Brush CreateBrushByMaterial(D2D1RenderTarget renderTarget, WindowMaterial material)
            {
                byte[] variantKeys = _variantKeys;
                if (variantKeys.Length <= 0)
                    return _base.Invoke(renderTarget);
                int index = Array.BinarySearch(variantKeys, (byte)material);
                if (index < 0)
                    return _base.Invoke(renderTarget);
                return _variants[index].Invoke(renderTarget);
            }

            public IEnumerable<WindowMaterial> GetVariants()
            {
                foreach (byte variant in _variantKeys)
                    yield return (WindowMaterial)variant;
            }

            internal Func<D2D1RenderTarget, D2D1Brush> Destruct(out byte[] variantKeys, out Func<D2D1RenderTarget, D2D1Brush>[] variantBrushFactories)
            {
                variantKeys = _variantKeys;
                variantBrushFactories = _variants;
                return _base;
            }
        }
    }
}
