using System;

using ConcreteUI.Native;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using Microsoft.Win32;

using static ConcreteUI.Native.DwmApi;
using static ConcreteUI.Native.UxTheme;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class FluentHandler
    {
        public static unsafe DWMSystemBackdropType GetCurrentBackdrop(IntPtr Handle)
        {
            DWMSystemBackdropType v;
            if (DwmGetWindowAttribute(Handle, DWMWindowAttribute.SystemBackdropType, &v, sizeof(DWMSystemBackdropType)) == 0)
            {
                return v;
            }
            else
            {
                return DWMSystemBackdropType.None;
            }
        }

        public static unsafe void EnableMicaBackdrop(IntPtr Handle)
        {
            DWMSystemBackdropType v = DWMSystemBackdropType.MainWindow;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.SystemBackdropType, &v, sizeof(DWMSystemBackdropType));
        }

        public static unsafe void EnableMicaAltBackdrop(IntPtr Handle)
        {
            DWMSystemBackdropType v = DWMSystemBackdropType.TabbedWindow;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.SystemBackdropType, &v, sizeof(DWMSystemBackdropType));
        }

        public static unsafe void EnableAcrylicBackdrop(IntPtr Handle)
        {
            DWMSystemBackdropType v = DWMSystemBackdropType.TransientWindow;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.SystemBackdropType, &v, sizeof(DWMSystemBackdropType));
        }

        public static unsafe void DisableBackdrop(IntPtr Handle)
        {
            DWMSystemBackdropType v = DWMSystemBackdropType.Auto;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.SystemBackdropType, &v, sizeof(DWMSystemBackdropType));
        }

        public static unsafe void SetTitleBarColor(IntPtr Handle, in System.Drawing.Color color)
        {
            uint colorRaw;
            if (color.A == 0)
                colorRaw = 0xFFFFFFFF;
            else
                colorRaw = color.ToBgr();
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.CaptionColor, &colorRaw, sizeof(DWMSystemBackdropType));
        }

        public static unsafe void SetDarkThemeInWin11(IntPtr Handle, bool value)
        {
            int val = value ? 0x1 : 0x0;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.UseImmersiveDarkMode, &val, sizeof(int));
            SetPreferredAppMode(value ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);
        }

        public static unsafe void SetDarkThemeInWin10_19H1(bool value)
        {
            SetPreferredAppMode(value ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);
        }

        public static bool CheckAppsUseLightTheme()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key is null) return true;
            else
            {
                bool result = key.GetValue("AppsUseLightTheme", 0x1) is int value && value == 0x1;
                key.Dispose();
                return result;
            }
        }

        public static unsafe void EnableBlur(IntPtr Handle)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableBlurBehind,
                AnimationId = 0,
                AccentFlags = AccentFlags.None
            };
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };

            User32.SetWindowCompositionAttribute(Handle, &data);
        }

        public static unsafe void EnableAcrylicBlur(IntPtr Handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableAcrylicBlurBehind,
                AnimationId = 0,
                AccentFlags = AccentFlags.None
            };
            if (isDarkMode)
                accent.GradientColor = System.Drawing.Color.FromArgb(0, 0, 0, 100).ToBgra();
            else
                accent.GradientColor = System.Drawing.Color.FromArgb(255, 254, 247, 100).ToBgra();
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };

            User32.SetWindowCompositionAttribute(Handle, &data);
        }

        public static unsafe void DisableBlur(IntPtr Handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableGradient,
                AnimationId = 0,
                AccentFlags = AccentFlags.None
            };
            if (isDarkMode)
                accent.GradientColor = System.Drawing.Color.FromArgb(0, 0, 0, 100).ToBgra();
            else
                accent.GradientColor = System.Drawing.Color.FromArgb(255, 254, 247, 100).ToBgra();
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };
            User32.SetWindowCompositionAttribute(Handle, &data);
        }

        public static unsafe void ApplyWin11Corner(IntPtr Handle)
        {
            DWMWindowCornerPreference v = DWMWindowCornerPreference.Round;
            DwmSetWindowAttribute(Handle, DWMWindowAttribute.WindowCornerPreference, &v, sizeof(DWMWindowCornerPreference));
        }

        public static object FixLagForAcrylic(System.Windows.Forms.Form form)
        {
            return new AcrylicLagFixFor21H2(form, true);
        }

        public static object FixLagForBlur(System.Windows.Forms.Form form)
        {
            return new AcrylicLagFixFor21H2(form, false);
        }

        private class AcrylicLagFixFor21H2
        {
            bool isResizing = false;
            bool blur;

            public AcrylicLagFixFor21H2(System.Windows.Forms.Form window, bool blur)
            {
                window.Resize += Resize;
                window.ResizeBegin += ResizeBegin;
                window.ResizeEnd += ResizeEnd;
                window.FormClosing += FormClosing;
                this.blur = blur;
            }

            private void FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
            {
                System.Windows.Forms.Form window = (System.Windows.Forms.Form)sender;
                window.Resize -= Resize;
                window.ResizeBegin -= ResizeBegin;
                window.ResizeEnd -= ResizeEnd;
                window.FormClosing -= FormClosing;
            }

            private void Resize(object sender, EventArgs e)
            {
                if (!isResizing || sender is not CoreWindow window)
                    return;
                DisableBlur(window.Handle, window.Theme.IsDarkTheme);
            }

            private void ResizeBegin(object sender, EventArgs e)
            {
                isResizing = true;
            }

            private void ResizeEnd(object sender, EventArgs e)
            {
                if (!isResizing || sender is not CoreWindow window)
                    return;
                isResizing = false;
                IntPtr handle = window.Handle;
                if (blur)
                    EnableAcrylicBlur(handle, window.Theme.IsDarkTheme);
                else
                    EnableBlur(handle);
            }
        }
    }
}
