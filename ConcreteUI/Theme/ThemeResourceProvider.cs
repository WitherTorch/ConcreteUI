using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Utils;
using ConcreteUI.Window;

using InlineMethod;

namespace ConcreteUI.Theme
{
    public static partial class ThemeResourceProvider
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static IThemeResourceProvider CreateResourceProvider(CoreWindow window, IThemeContext themeContext)
            => CreateResourceProvider(window.GetDeviceContext(), themeContext, window.WindowMaterial);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IThemeResourceProvider CreateResourceProvider(D2D1RenderTarget renderTarget, IThemeContext themeContext, WindowMaterial windowMaterial)
            => new ThemeResourceProviderImpl(renderTarget, themeContext,
                (windowMaterial < WindowMaterial.None || windowMaterial >= WindowMaterial._Last) ? SystemHelper.GetDefaultMaterial() : windowMaterial);
    }
}
