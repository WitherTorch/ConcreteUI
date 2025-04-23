using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using ConcreteUI.Window;

namespace ConcreteUI.Theme
{
    public delegate void ThemeChangingEventHandler(IThemeContext context);

    public static class ThemeManager
    {
        private static readonly ConcurrentDictionary<string, IThemeContext?> _themeDict = new ConcurrentDictionary<string, IThemeContext?>();
        private static readonly HashSet<IThemeProvider> _providers = new HashSet<IThemeProvider>();
        private static IThemeContext _currentTheme;

        public static event ThemeChangingEventHandler? OnThemeChanging;
        public static event EventHandler? OnThemeChanged;

        public static IThemeContext CurrentTheme
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentTheme;
            set
            {
                if (ReferenceEquals(_currentTheme, value))
                    return;
                OnThemeChanging?.Invoke(value);
                _currentTheme = value;
                OnThemeChanged?.Invoke(null, EventArgs.Empty);
                CoreWindow.NotifyThemeChanged(value);
            }
        }

        static ThemeManager()
        {
            DefaultThemeProvider provider = DefaultThemeProvider.Instance;
            _providers.Add(provider);
            _currentTheme = provider.LightTheme;
        }

        public static void RegisterThemeProvider(IThemeProvider provider)
        {
            HashSet<IThemeProvider> providers = _providers;
            lock (providers)
                providers.Add(provider);
        }

        public static void UnregisterThemeProvider(IThemeProvider provider)
        {
            HashSet<IThemeProvider> providers = _providers;
            lock (providers)
                providers.Remove(provider);
        }

        public static bool TryGetThemeContext(string themeId, [NotNullWhen(true)] out IThemeContext? theme)
        {
            ConcurrentDictionary<string, IThemeContext?> themeDict = _themeDict;
            theme = themeDict.AddOrUpdate(themeId, FindThemeContext, UpdateThemeContext);
            return theme is not null;
        }

        private static IThemeContext? FindThemeContext(string themeId)
        {
            HashSet<IThemeProvider> providers = _providers;
            lock (providers)
            {
                foreach (IThemeProvider provider in providers)
                {
                    if (provider.TryGetTheme(themeId, out IThemeContext? theme))
                        return theme;
                }
            }
            return null;
        }

        private static IThemeContext? UpdateThemeContext(string themeId, IThemeContext? oldThemeContext)
        {
            if (oldThemeContext is not null)
                return oldThemeContext;
            return FindThemeContext(themeId);
        }
    }
}
