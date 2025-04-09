using System.Diagnostics.CodeAnalysis;

namespace ConcreteUI.Theme
{
    public interface IThemeProvider
    {
        bool TryGetTheme(string themeId, [NotNullWhen(true)] out IThemeContext? theme);
    }
}
