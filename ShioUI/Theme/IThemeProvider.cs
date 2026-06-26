using System.Diagnostics.CodeAnalysis;

namespace ShioUI.Theme;

public interface IThemeProvider
{
    bool TryGetTheme(string themeId, [NotNullWhen(true)] out IThemeContext? theme);
}
