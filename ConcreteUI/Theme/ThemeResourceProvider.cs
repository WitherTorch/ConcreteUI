using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

namespace ConcreteUI.Theme
{
    public static partial class ThemeResourceProvider
    {
        public static readonly IThemeResourceProvider Empty = new EmptyImpl();

        [Inline(InlineBehavior.Keep, export: true)]
        public static IThemeResourceProvider CreateResourceProvider(CoreWindow window, IThemeContext themeContext)
            => CreateResourceProviderUnsafe(window.GetDeviceContext(), themeContext, window.ActualWindowMaterial);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemeResourceProvider CreateResourceProvider(D2D1DeviceContext deviceContext, IThemeContext themeContext, WindowMaterial windowMaterial)
            => CreateResourceProviderUnsafe(deviceContext, themeContext, SystemHelper.GetActualWindowMaterial(windowMaterial));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IThemeResourceProvider CreateResourceProviderUnsafe(D2D1DeviceContext deviceContext, IThemeContext themeContext, WindowMaterial windowMaterial)
            => new NormalImpl(deviceContext, themeContext, windowMaterial);
    }
}
