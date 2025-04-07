namespace ConcreteUI.Theme
{
    public interface IThemeContext
    {
        bool IsDarkTheme { get; }

        IThemeContext Clone();

        bool TryGetColorFactory(string node, out IThemedColorFactory colorFactory);

        bool TryGetBrushFactory(string node, out IThemedBrushFactory brushFactory);

        bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides);

        bool TrySetBrushFactory(string node, IThemedBrushFactory colorFactory, bool overrides);

        void ApplyToOtherContext(IThemeContext other, bool overrides);
    }
}
