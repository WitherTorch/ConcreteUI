using System.Diagnostics.CodeAnalysis;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Theme
{
    internal sealed partial class DefaultThemeProvider : IThemeProvider
    {
        private static readonly DefaultThemeProvider _instance = new DefaultThemeProvider();

        public static DefaultThemeProvider Instance => _instance;
        public static IThemeContext LightTheme => LightThemeContext.Instance;
        public static IThemeContext DarkTheme => DarkThemeContext.Instance;

        private DefaultThemeProvider() { }

        public bool TryGetTheme(string themeId, [NotNullWhen(true)] out IThemeContext? theme)
        {
            theme = themeId.ToLowerAscii() switch
            {
                "#light" => LightThemeContext.Instance,
                "#dark" => DarkThemeContext.Instance,
                _ => null
            };
            return theme is not null;
        }
    }
}
