using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Theme
{
    partial class ThemedColorFactory
    {
        private sealed class ThemedColorFactoryImpl : IThemedColorFactory
        {
            private readonly D2D1ColorF _base;
            private readonly byte[] _variantKeys;
            private readonly D2D1ColorF[] _variants;

            internal ThemedColorFactoryImpl(in D2D1ColorF baseColor, byte[] variantKeys, D2D1ColorF[] variantColors)
            {
                if (variantKeys.Length != variantColors.Length)
                    throw new InvalidOperationException();
                Array.Sort(variantKeys, variantColors);
                _base = baseColor;
                _variantKeys = variantKeys;
                _variants = variantColors;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public D2D1ColorF CreateDefaultColor() => _base;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public D2D1ColorF CreateColorByMaterial(WindowMaterial material)
            {
                byte[] variantKeys = _variantKeys;
                if (variantKeys.Length <= 0)
                    return _base;
                int index = Array.BinarySearch(variantKeys, (byte)material);
                if (index < 0)
                    return _base;
                return _variants[index];
            }

            public IEnumerable<WindowMaterial> GetVariants()
            {
                foreach (byte variant in _variantKeys)
                    yield return (WindowMaterial)variant;
            }

            internal D2D1ColorF Destruct(out byte[] variantKeys, out D2D1ColorF[] variantColors)
            {
                variantKeys = _variantKeys;
                variantColors = _variants;
                return _base;
            }
        }
    }
}
