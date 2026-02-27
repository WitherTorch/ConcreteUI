using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals;
using ConcreteUI.Internals.Native;
using ConcreteUI.Window;

using InlineMethod;

namespace ConcreteUI.Theme
{
    public delegate void ThemeChangingEventHandler(IThemeContext context);

    public static class ThemeManager
    {
        private static readonly ConcurrentDictionary<string, IThemeContext?> _themeDict = new ConcurrentDictionary<string, IThemeContext?>();
        private static readonly HashSet<IThemeProvider> _providers = new HashSet<IThemeProvider>();
        private static IThemeContext _currentTheme;

        public static event ThemeChangingEventHandler? ThemeChanging;
        public static event EventHandler? ThemeChanged;

        public static IThemeContext CurrentTheme
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentTheme;
            set
            {
                if (ReferenceEquals(_currentTheme, value))
                    return;
                OnThemeChanging(value);
                _currentTheme = value;
                OnThemeChanged(value);
            }
        }

        static ThemeManager()
        {
            DefaultThemeProvider provider = DefaultThemeProvider.Instance;
            _providers.Add(provider);
            IThemeContext context = provider.LightTheme;
            _currentTheme = context;
            UpdateDarkModeState(context);
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

        [Inline(InlineBehavior.Remove)]
        private static void OnThemeChanging(IThemeContext context) => ThemeChanging?.Invoke(context);

        [Inline(InlineBehavior.Remove)]
        private static void OnThemeChanged(IThemeContext context)
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
            CoreWindow.NotifyThemeChanged(context);
            UpdateDarkModeState(context);
        }

        [Inline(InlineBehavior.Remove)]
        private static void UpdateDarkModeState(IThemeContext context)
        {
            if (SystemConstants.VersionLevel >= SystemVersionLevel.Windows_10_19H1)
                UxTheme.SetPreferredAppMode(context.IsDarkTheme ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);
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
