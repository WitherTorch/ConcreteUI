using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Utils;

namespace ConcreteUI.Theme
{
    partial class ThemedBrushFactory
    {
        private sealed class FunctionThemedBrushFactoryImpl : IThemedBrushFactory
        {
            private readonly Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> _func;

            public FunctionThemedBrushFactoryImpl(Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> func)
            {
                _func = func;
            }

            public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context) => _func(context, WindowMaterial.Default);
            public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material) => _func(context, material);
            public IEnumerable<WindowMaterial> GetVariants() => SystemHelper.GetAvailableMaterials();
        }
    }
}
