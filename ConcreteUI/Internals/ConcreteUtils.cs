using System;
using System.Drawing;

using ConcreteUI.Internals.NativeHelpers;
using ConcreteUI.Native;
using ConcreteUI.Window;

namespace ConcreteUI.Internals
{
    internal static class ConcreteUtils
    {
        public static unsafe void ApplyWindowStyle(CoreWindow window, out object? fixLagObject)
        {
            IntPtr handle = window.Handle;
            WindowMaterial material = window.WindowMaterial;
            fixLagObject = null;
            switch (SystemConstants.VersionLevel)
            {
                case SystemVersionLevel.Windows_11_After: // Acrylic-theme-v2
                    switch (material)
                    {
                        case WindowMaterial.MicaAlt:
                            FluentHandler.EnableMicaAltBackdrop(handle);
                            break;
                        case WindowMaterial.Mica:
                            FluentHandler.EnableMicaBackdrop(handle);
                            break;
                        case WindowMaterial.Acrylic:
                            FluentHandler.EnableAcrylicBackdrop(handle);
                            break;
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            fixLagObject = FluentHandler.FixLagForBlur(window);
                            break;
                        case WindowMaterial.Integrated:
                            if (FluentHandler.GetCurrentBackdrop(handle) == DwmSystemBackdropType.Auto) //Soft apply backdrops, prevent overrided Mica for everyone
                            {
                                if (window is TabbedWindow)
                                    FluentHandler.EnableMicaAltBackdrop(handle);
                                else
                                    FluentHandler.EnableMicaBackdrop(handle);
                            }
                            window.BackColor = Color.Black;
                            break;
                    }
                    FluentHandler.ApplyWin11Corner(handle);
                    FluentHandler.SetDarkThemeInWin11(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                    break;
                case SystemVersionLevel.Windows_11_21H2: // Acrylic-theme
                    bool isDarkTheme = window.CurrentTheme?.IsDarkTheme ?? false;
                    switch (material)
                    {
                        case WindowMaterial.Acrylic:
                            FluentHandler.EnableAcrylicBlur(handle, isDarkTheme);
                            fixLagObject = FluentHandler.FixLagForAcrylic(window);
                            break;
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            fixLagObject = FluentHandler.FixLagForBlur(window);
                            break;
                        case WindowMaterial.Integrated:
                            window.BackColor = Color.Black;
                            break;
                    }
                    FluentHandler.ApplyWin11Corner(handle);
                    FluentHandler.SetDarkThemeInWin11(handle, isDarkTheme);
                    break;
                case SystemVersionLevel.Windows_10_19H1: // WindowMaterial-theme-v3
                    isDarkTheme = window.CurrentTheme?.IsDarkTheme ?? false;
                    switch (material)
                    {
                        case WindowMaterial.Acrylic:
                            FluentHandler.EnableAcrylicBlur(handle, isDarkTheme);
                            fixLagObject = FluentHandler.FixLagForAcrylic(window);
                            goto default;
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            goto default;
                        default:
                            window.BackColor = Color.Black;
                            break;
                    }
                    FluentHandler.SetDarkThemeInWin10_19H1(isDarkTheme);
                    break;
                case SystemVersionLevel.Windows_10_Redstone_4: // WindowMaterial-theme-v2
                    switch (material)
                    {
                        case WindowMaterial.Acrylic:
                            FluentHandler.EnableAcrylicBlur(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                            fixLagObject = FluentHandler.FixLagForAcrylic(window);
                            goto default;
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            goto default;
                        default:
                            window.BackColor = Color.Black;
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_10: // WindowMaterial-theme
                    switch (material)
                    {
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            goto default;
                        default:
                            window.BackColor = Color.Black;
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_8:
                case SystemVersionLevel.Windows_7:
                    switch (material)
                    {
                        case WindowMaterial.Integrated:
                            if (AeroHandler.HasBlur())
                                AeroHandler.EnableBlur(handle);
                            window.BackColor = Color.Black;
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        public static void ResetBlur(CoreWindow window)
        {
            IntPtr handle = window.Handle;
            switch (SystemConstants.VersionLevel)
            {
                case SystemVersionLevel.Windows_11_After:
                    FluentHandler.SetDarkThemeInWin11(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                    switch (window.WindowMaterial)
                    {
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_11_21H2:
                    FluentHandler.SetDarkThemeInWin11(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                    goto case SystemVersionLevel.Reserved_1;
                case SystemVersionLevel.Windows_10_19H1:
                    FluentHandler.SetDarkThemeInWin10_19H1(window.CurrentTheme?.IsDarkTheme ?? false);
                    goto case SystemVersionLevel.Reserved_1;
                case SystemVersionLevel.Windows_10_Redstone_4:
                case SystemVersionLevel.Windows_10:
                    goto case SystemVersionLevel.Reserved_1;
                case SystemVersionLevel.Reserved_1:
                    switch (window.WindowMaterial)
                    {
                        case WindowMaterial.Gaussian:
                            FluentHandler.EnableBlur(handle);
                            break;
                        case WindowMaterial.Acrylic:
                            FluentHandler.EnableAcrylicBlur(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_8:
                case SystemVersionLevel.Windows_7:
                    if (window.WindowMaterial == WindowMaterial.Integrated)
                        if (AeroHandler.HasBlur())
                            AeroHandler.EnableBlur(handle);
                    break;
            }
        }
    }
}
