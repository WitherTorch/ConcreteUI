using ConcreteUI.Internals;

namespace ConcreteUI.Utils
{
    public static class SystemHelper
    {
        private static WindowMaterial[] _availableMaterials = GetAvailableMaterialsCore();

        public static WindowMaterial[] GetAvailableMaterials() => _availableMaterials;

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
    }
}
