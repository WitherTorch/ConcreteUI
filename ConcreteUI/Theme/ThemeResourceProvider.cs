using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;

using ConcreteUI.Window;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using System.Runtime.CompilerServices;
using InlineMethod;

namespace ConcreteUI.Theme
{
    public static partial class ThemeResourceProvider
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static IThemeResourceProvider CreateResourceProvider(CoreWindow window, IThemeContext themeContext) 
            => CreateResourceProvider(window.GetDeviceContext(), themeContext, window.WindowMaterial);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemeResourceProvider CreateResourceProvider(D2D1DeviceContext deviceContext, IThemeContext themeContext, WindowMaterial windowMaterial) 
            => new ThemeResourceProviderImpl(deviceContext, themeContext, windowMaterial);
    }
}
