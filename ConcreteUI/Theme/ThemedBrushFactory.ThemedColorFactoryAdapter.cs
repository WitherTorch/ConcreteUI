using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    partial class ThemedBrushFactory
    {
        private sealed class ThemedColorFactoryAdapter : IThemedBrushFactory, IThemedColorFactory
        {
            private readonly IThemedColorFactory _factory;

            public ThemedColorFactoryAdapter(IThemedColorFactory factory)
            {
                _factory = factory;
            }

            public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material)
                => context.CreateSolidColorBrush(_factory.CreateColorByMaterial(material));

            public D2D1ColorF CreateColorByMaterial(WindowMaterial material) => _factory.CreateColorByMaterial(material);

            public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context) => context.CreateSolidColorBrush(_factory.CreateDefaultColor());

            public D2D1ColorF CreateDefaultColor() => _factory.CreateDefaultColor();

            public IEnumerable<WindowMaterial> GetVariants() => _factory.GetVariants();
        }
    }
}
