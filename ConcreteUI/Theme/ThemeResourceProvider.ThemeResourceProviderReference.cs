using System.Diagnostics.CodeAnalysis;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    partial class ThemeResourceProvider
    {
        private sealed class ThemeResourceProviderReference : IThemeResourceProvider
        {
            private readonly IThemeResourceProvider _provider;

            public ThemeResourceProviderReference(IThemeResourceProvider provider)
            {
                _provider = provider;
            }

            public string FontName => _provider.FontName;

            public IThemeContext ThemeContext => _provider.ThemeContext;

            public IThemeResourceProvider Clone() => this;

            public bool TryGetBrush(string node, [NotNullWhen(true)] out D2D1Brush? brush)
                => _provider.TryGetBrush(node, out brush);

            public bool TryGetColor(string node, out D2D1ColorF color)
                => _provider.TryGetColor(node, out color);
        }
    }
}
