using System.Collections.Generic;
using System.Linq;

using ConcreteUI.Graphics.Native.Direct2D;

namespace ConcreteUI.Theme
{

    partial class ThemedColorFactory
    {
        private sealed class SimpleThemedColorFactoryImpl : IThemedColorFactory
        {
            private readonly D2D1ColorF _color;

            public SimpleThemedColorFactoryImpl(in D2D1ColorF color) => _color = color;

            public D2D1ColorF CreateColorByMaterial(WindowMaterial material) => _color;

            public D2D1ColorF CreateDefaultColor() => _color;

            public IEnumerable<WindowMaterial> GetVariants() => Enumerable.Empty<WindowMaterial>();

            internal D2D1ColorF Destruct() => _color;
        }
    }
}
