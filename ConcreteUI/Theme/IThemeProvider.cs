namespace ConcreteUI.Theme
{
    public interface IThemeProvider
    {
        bool TryGetTheme(string themeId, out IThemeContext theme);
    }
}
