using System.Diagnostics.CodeAnalysis;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Theme
{
    public sealed partial class DefaultThemeProvider : IThemeProvider
    {
        private static readonly DefaultThemeProvider _instance = new DefaultThemeProvider();

        private readonly LightThemeContext _lightTheme = new LightThemeContext();
        private readonly DarkThemeContext _darkTheme = new DarkThemeContext();

        public static DefaultThemeProvider Instance => _instance;
        public IThemeContext LightTheme => _lightTheme;
        public IThemeContext DarkTheme => _darkTheme;

        private DefaultThemeProvider() { }

        bool IThemeProvider.TryGetTheme(string themeId, [NotNullWhen(true)] out IThemeContext? theme)
        {
            theme = themeId.ToLowerAscii() switch
            {
                "#light" => _lightTheme,
                "#dark" => _darkTheme,
                _ => null
            };
            return theme is not null;
        }
    }
}
