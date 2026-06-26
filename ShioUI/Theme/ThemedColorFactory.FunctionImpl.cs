using System;
using System.Collections.Generic;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Utils;

namespace ShioUI.Theme;

partial class ThemedColorFactory
{
    private sealed class FunctionImpl : IThemedColorFactory
    {
        private readonly Func<WindowMaterial, D2D1ColorF> _func;

        public FunctionImpl(Func<WindowMaterial, D2D1ColorF> func)
        {
            _func = func;
        }

        public D2D1ColorF CreateColorByMaterial(WindowMaterial material) => _func(material);

        public D2D1ColorF CreateDefaultColor() => _func(WindowMaterial.Default);

        public IEnumerable<WindowMaterial> GetVariants() => SystemHelper.GetAvailableMaterials();
    }
}
