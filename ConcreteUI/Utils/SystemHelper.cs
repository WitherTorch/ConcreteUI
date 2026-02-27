using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

namespace ConcreteUI.Utils
{
    public static class SystemHelper
    {
        private static readonly WindowMaterial[] _availableMaterials = GetAvailableMaterialsCore();
        private static readonly WindowMaterial _defaultMaterial = GetDefaultMaterialCore();

        public static WindowMaterial[] GetAvailableMaterials() => _availableMaterials;

        public static WindowMaterial GetDefaultMaterial() => _defaultMaterial;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows7() => SystemConstants.VersionLevel == SystemVersionLevel.Windows_7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows7OrHigher() => SystemConstants.VersionLevel >= SystemVersionLevel.Windows_7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows8() => SystemConstants.VersionLevel == SystemVersionLevel.Windows_8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows8OrHigher() => SystemConstants.VersionLevel >= SystemVersionLevel.Windows_8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows10() => SystemConstants.VersionLevel switch
        {
            SystemVersionLevel.Windows_10_Redstone_4 or SystemVersionLevel.Windows_10_19H1 or
            SystemVersionLevel.Windows_10 => true,
            _ => false
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows10OrHigher() => SystemConstants.VersionLevel >= SystemVersionLevel.Windows_10;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows11() => SystemConstants.VersionLevel switch
        {
            SystemVersionLevel.Windows_11_21H2 or SystemVersionLevel.Windows_11_After => true,
            _ => false
        };

        private static WindowMaterial[] GetAvailableMaterialsCore()
            => SystemConstants.VersionLevel switch
            {
                SystemVersionLevel.Windows_11_After => [
                    WindowMaterial.MicaAlt,
                    WindowMaterial.Mica,
                    WindowMaterial.Acrylic,
                    WindowMaterial.Gaussian,
                    WindowMaterial.Integrated,
                    WindowMaterial.None
                ],
                SystemVersionLevel.Windows_11_21H2 or SystemVersionLevel.Windows_10_19H1 or SystemVersionLevel.Windows_10_Redstone_4 => [
                    WindowMaterial.Acrylic,
                    WindowMaterial.Gaussian,
                    WindowMaterial.Integrated,
                    WindowMaterial.None
                ],
                SystemVersionLevel.Windows_10 => [
                    WindowMaterial.Gaussian,
                    WindowMaterial.Integrated,
                    WindowMaterial.None
                ],
                SystemVersionLevel.Windows_8 or SystemVersionLevel.Windows_7 => [
                    WindowMaterial.Integrated,
                    WindowMaterial.None
                ],
                _ => [WindowMaterial.None]
            };

        private static WindowMaterial GetDefaultMaterialCore()
            => SystemConstants.VersionLevel switch
            {
                SystemVersionLevel.Windows_11_After or SystemVersionLevel.Windows_11_21H2 => WindowMaterial.Acrylic,
                SystemVersionLevel.Windows_10_19H1 or SystemVersionLevel.Windows_10_Redstone_4 or
                SystemVersionLevel.Windows_10 => WindowMaterial.Gaussian,
                SystemVersionLevel.Windows_8 or SystemVersionLevel.Windows_7 => WindowMaterial.Integrated,
                _ => WindowMaterial.None
            };
    }
}
