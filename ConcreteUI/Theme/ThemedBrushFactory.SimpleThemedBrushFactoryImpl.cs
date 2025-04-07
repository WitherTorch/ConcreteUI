﻿using System;
using System.Collections.Generic;
using System.Linq;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{

    partial class ThemedBrushFactory
    {
        private sealed class SimpleThemedBrushFactoryImpl : IThemedBrushFactory
        {
            private readonly Func<D2D1DeviceContext, D2D1Brush> _base;

            public SimpleThemedBrushFactoryImpl(Func<D2D1DeviceContext, D2D1Brush> baseBrushFactory)
            {
                _base = baseBrushFactory;
            }

            public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context) => _base.Invoke(context);

            public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material) => _base.Invoke(context);

            public IEnumerable<WindowMaterial> GetVariants() => Enumerable.Empty<WindowMaterial>();

            public Func<D2D1DeviceContext, D2D1Brush> Destruct() => _base;
        }
    }
}
