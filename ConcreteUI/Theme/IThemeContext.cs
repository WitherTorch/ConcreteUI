using System.Diagnostics.CodeAnalysis;

namespace ConcreteUI.Theme
{
    public interface IThemeContext
    {
        bool IsDarkTheme { get; }

        string FontName { get; set; }

        IThemeContext Clone();

        bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory);

        bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory);

        bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides);

        bool TrySetBrushFactory(string node, IThemedBrushFactory colorFactory, bool overrides);

        void BuildContextForAnother(IThemeContext other, bool overrides);
    }
}
