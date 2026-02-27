using System;
using System.Drawing;
using System.Runtime.CompilerServices;

using ConcreteUI.Internals.Native;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using Microsoft.Win32;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals.NativeHelpers
{
    internal static class FluentHandler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DwmSystemBackdropType GetCurrentBackdrop(IntPtr handle)
            => DwmApi.DwmGetWindowAttributeOrDefault(handle, DwmWindowAttribute.SystemBackdropType, DwmSystemBackdropType.None);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnableMicaBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.MainWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnableMicaAltBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.TabbedWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnableAcrylicBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.TransientWindow);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisableBackdrop(IntPtr handle)
            => SetBackdropType(handle, DwmSystemBackdropType.Auto);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBackdropType(IntPtr handle, DwmSystemBackdropType type)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.SystemBackdropType, type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTitleBarColor(IntPtr handle, in Color color)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.CaptionColor, color.A == 0 ? uint.MaxValue : color.ToBgr());

        public static void SetDarkThemeInWin11(IntPtr handle, SysBool value)
        {
            DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.UseImmersiveDarkMode, value);
        }

        public static unsafe void SetDarkThemeInWin10_19H1(IntPtr handle, SysBool value)
        {
            WindowCompositionAttributeData data = new WindowCompositionAttributeData()
            {
                Attribute = WindowCompositionAttribute.UseDarkModeColors,
                Data = &value,
                SizeOfData = sizeof(SysBool)
            };
            User32.SetWindowCompositionAttribute(handle, &data);
        }

        public static bool CheckAppsUseLightTheme()
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key is null)
                return true;
            bool result = key.GetValue("AppsUseLightTheme", 0x1) is int value && value == 0x1;
            key.Dispose();
            return result;
        }

        public static unsafe void EnableBlur(IntPtr handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableBlurBehind,
                AnimationId = 0,
                AccentFlags = AccentFlags.None,
                GradientColor = isDarkMode ? 0x66000000 : 0x66FFFFFFu
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
                AccentFlags = AccentFlags.None,
                GradientColor = isDarkMode ? 0x802B2B2B : 0x80F2F2F2
            };
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };

            User32.SetWindowCompositionAttribute(handle, &data);
        }

        public static unsafe void EnableSolidBackdrop(IntPtr handle, bool isDarkMode)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.EnableGradient,
                AnimationId = 0,
                AccentFlags = AccentFlags.None,
                GradientColor = isDarkMode ? 0xFF1A1A1Au : 0xFFF2F2F2u
            };
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                SizeOfData = sizeof(AccentPolicy),
                Data = &accent
            };
            User32.SetWindowCompositionAttribute(handle, &data);
        }

        public static unsafe void DisableBlur(IntPtr handle)
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.Disabled,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyWin11Corner(IntPtr handle)
            => DwmApi.DwmSetWindowAttribute(handle, DwmWindowAttribute.WindowCornerPreference, DwmWindowCornerPreference.Round);

        public static object? FixLagForAcrylic(CoreWindow window)
            => new VisualLagFixFilter(window, isAcrylic: true,
                fallBackToGaussian: !window.ExtendedStyles.HasFlagOptimized(WindowExtendedStyles.NoRedirectionBitmap));

        public static object? FixLagForBlur(CoreWindow window)
            => window.ExtendedStyles.HasFlagOptimized(WindowExtendedStyles.NoRedirectionBitmap) ? 
            new VisualLagFixFilter(window, isAcrylic: false, fallBackToGaussian: false) : null;

        private sealed class VisualLagFixFilter : IWindowMessageFilter, IDisposable
        {
            private readonly bool _isAcrylic;
            private readonly bool _fallBackToGaussian;
            private readonly WeakReference<CoreWindow> _windowReference;

            private bool _disposed;

            public VisualLagFixFilter(CoreWindow window, bool isAcrylic, bool fallBackToGaussian)
            {
                _windowReference = new WeakReference<CoreWindow>(window);
                _isAcrylic = isAcrylic;
                _fallBackToGaussian = fallBackToGaussian;
                _disposed = false;

                window.AddMessageFilter(this);
            }

            public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
            {
                result = 0;
                switch (message)
                {
                    case WindowMessage.EnterSizeMove:
                        {
                            if (!_windowReference.TryGetTarget(out CoreWindow? window))
                                break;
                            bool isDarkMode = window.CurrentTheme?.IsDarkTheme ?? false;
                            if (_fallBackToGaussian)
                                EnableBlur(hwnd, isDarkMode);
                            else
                                EnableSolidBackdrop(hwnd, isDarkMode);
                        }
                        break;
                    case WindowMessage.ExitSizeMove:
                        {
                            if (!_windowReference.TryGetTarget(out CoreWindow? window))
                                break;
                            bool isDarkMode = window.CurrentTheme?.IsDarkTheme ?? false;
                            DisableBlur(hwnd);
                            User32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, 
                                WindowPositionFlags.SwapWithNoMove | WindowPositionFlags.SwapWithNoSize | 
                                WindowPositionFlags.SwapWithNoZOrder | WindowPositionFlags.SwapWithFrameChanged);
                            if (_isAcrylic)
                                EnableAcrylicBlur(hwnd, isDarkMode);
                            else
                                EnableBlur(hwnd, isDarkMode);
                        }
                        break;
                    case WindowMessage.Destroy:
                        Dispose();
                        break;
                }
                return false;
            }

            private void DisposeCore()
            {
                if (ReferenceHelper.Exchange(ref _disposed, true) || !_windowReference.TryGetTarget(out CoreWindow? window))
                    return;
                window.RemoveMessageFilter(this);
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }
        }
    }
}
