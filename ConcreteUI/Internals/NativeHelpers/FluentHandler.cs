﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Native;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using Microsoft.Win32;

using WitherTorch.Common.Windows.Structures;

using Form = System.Windows.Forms.Form;
using FormClosingEventArgs = System.Windows.Forms.FormClosingEventArgs;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class FluentHandler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe DwmSystemBackdropType GetCurrentBackdrop(IntPtr handle)
            => DwmApi.DwmGetWindowAttributeOrDefault(handle, DwmWindowAttribute.SystemBackdropType, DwmSystemBackdropType.None);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EnableMicaBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.MainWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EnableMicaAltBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.TabbedWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EnableAcrylicBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.TransientWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void DisableBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.Auto);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void SetBackdropType(IntPtr handle, DwmSystemBackdropType type)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.SystemBackdropType, type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetTitleBarColor(IntPtr handle, in Color color)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.CaptionColor, color.A == 0 ? uint.MaxValue : color.ToBgr());

        public static unsafe void SetDarkThemeInWin11(IntPtr handle, bool value)
        {
            DwmApi.DwmSetWindowAttribute<SysBool>(handle, DwmWindowAttribute.UseImmersiveDarkMode, value);
            UxTheme.SetPreferredAppMode(value ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);
        }

        public static unsafe void SetDarkThemeInWin10_19H1(bool value)
            => UxTheme.SetPreferredAppMode(value ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);

        public static bool CheckAppsUseLightTheme()
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key is null)
                return true;
            bool result = key.GetValue("AppsUseLightTheme", 0x1) is int value && value == 0x1;
            key.Dispose();
            return result;
        }

        public static unsafe void EnableBlur(IntPtr handle)
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

            User32.SetWindowCompositionAttribute(handle, &data);
        }

        public static unsafe void EnableAcrylicBlur(IntPtr handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableAcrylicBlurBehind,
                AnimationId = 0,
                AccentFlags = AccentFlags.None
            };
            if (isDarkMode)
                accent.GradientColor = Color.FromArgb(0, 0, 0, 100).ToBgra();
            else
                accent.GradientColor = Color.FromArgb(255, 254, 247, 100).ToBgra();
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };

            User32.SetWindowCompositionAttribute(handle, &data);
        }

        public static unsafe void DisableBlur(IntPtr handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableGradient,
                AnimationId = 0,
                AccentFlags = AccentFlags.None
            };
            if (isDarkMode)
                accent.GradientColor = Color.FromArgb(0, 0, 0, 100).ToBgra();
            else
                accent.GradientColor = Color.FromArgb(255, 254, 247, 100).ToBgra();
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };
            User32.SetWindowCompositionAttribute(handle, &data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ApplyWin11Corner(IntPtr handle)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.WindowCornerPreference, DwmWindowCornerPreference.Round);

        public static object FixLagForAcrylic(Form form) 
            => new AcrylicLagFixFor21H2(form, isAcrylic: true);

        public static object FixLagForBlur(Form form) 
            => new AcrylicLagFixFor21H2(form, isAcrylic: false);

        private sealed class AcrylicLagFixFor21H2
        {
            private readonly bool _isAcrylic;

            private bool _isResizing;

            public AcrylicLagFixFor21H2(Form window, bool isAcrylic)
            {
                _isResizing = false;
                _isAcrylic = isAcrylic;
                window.Resize += Resize;
                window.ResizeBegin += ResizeBegin;
                window.ResizeEnd += ResizeEnd;
                window.FormClosing += FormClosing;
            }

            private void FormClosing(object? sender, FormClosingEventArgs e)
            {
                if (sender is not CoreWindow window)
                    return;
                window.Resize -= Resize;
                window.ResizeBegin -= ResizeBegin;
                window.ResizeEnd -= ResizeEnd;
                window.FormClosing -= FormClosing;
            }

            private void Resize(object? sender, EventArgs e)
            {
                if (!_isResizing || sender is not CoreWindow window)
                    return;
                DisableBlur(window.Handle, window.CurrentTheme?.IsDarkTheme ?? false);
            }

            private void ResizeBegin(object? sender, EventArgs e) 
                => _isResizing = true;

            private void ResizeEnd(object? sender, EventArgs e)
            {
                if (!_isResizing || sender is not CoreWindow window)
                    return;
                _isResizing = false;
                IntPtr handle = window.Handle;
                if (_isAcrylic)
                    EnableAcrylicBlur(handle, window.CurrentTheme?.IsDarkTheme ?? false);
                else
                    EnableBlur(handle);
            }
        }
    }
}
