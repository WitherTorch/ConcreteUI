using System.Diagnostics.CodeAnalysis;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    partial class ThemeResourceProvider
    {
        private sealed class EmptyImpl : IThemeResourceProvider
        {
            public string FontName => string.Empty;

            public IThemeContext ThemeContext => EmptyThemeContext.Instance;

            public IThemeResourceProvider Clone() => this;

            public bool TryGetBrush(string node, [NotNullWhen(true)] out D2D1Brush? brush)
            {
                brush = null;
                return false;
            }

            public bool TryGetColor(string node, out D2D1ColorF color)
            {
                color = default;
                return false;
            }
        }
    }
}
