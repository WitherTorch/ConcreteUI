using System;
using System.Collections.Generic;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Utils;

namespace ShioUI.Theme;

partial class ThemedBrushFactory
{
    private sealed class FunctionImpl : IThemedBrushFactory
    {
        private readonly Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> _func;

        public FunctionImpl(Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> func)
        {
            _func = func;
        }

        public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context) => _func(context, WindowMaterial.Default);
        public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material) => _func(context, material);
        public IEnumerable<WindowMaterial> GetVariants() => SystemHelper.GetAvailableMaterials();
    }
}
