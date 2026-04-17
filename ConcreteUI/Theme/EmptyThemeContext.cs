using System.Diagnostics.CodeAnalysis;

namespace ConcreteUI.Theme
{
    public sealed class EmptyThemeContext : IThemeContext
    {
        public static readonly EmptyThemeContext Instance = new EmptyThemeContext();

        private EmptyThemeContext() { }

        public string FontName
        {
            get => string.Empty;
            set { }
        }

        public bool IsDarkTheme => false;

        public void BuildContextForAnother(IThemeContext other, bool overrides) { }

        public IThemeContext Clone() => this;

        public bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory)
        {
            brushFactory = null;
            return false;
        }

        public bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory)
        {
            colorFactory = null;
            return false;
        }

        public bool TrySetBrushFactory(string node, IThemedBrushFactory brushFactory, bool overrides)
        {
            return false;
        }

        public bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides)
        {
            return false;
        }
    }
}
