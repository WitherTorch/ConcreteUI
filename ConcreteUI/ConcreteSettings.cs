using System.Runtime.CompilerServices;

using ConcreteUI.Internals;

namespace ConcreteUI
{
    public static class ConcreteSettings
    {
        public const string ReservedGpuName_None = "#none";
        public const string ReservedGpuName_Default = "#default";
        public const string ReservedGpuName_MinimumPower = "#default_minimum_power";
        public const string ReservedGpuName_HighPerformance = "#default_high_performance";

        private static WindowMaterial _windowMaterial = WindowMaterial.None;

        public static bool UseDebugMode { get; set; } = false;
        public static string TargetGpuName { get; set; } = ReservedGpuName_Default;
        public static WindowMaterial WindowMaterial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _windowMaterial;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                WindowMaterial oldMaterial = _windowMaterial;
                if (oldMaterial == value)
                    return;
                _windowMaterial = RedirectWindowMaterialForSystemVersionLevel(value, SystemConstants.VersionLevel);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WindowMaterial RedirectWindowMaterialForSystemVersionLevel(WindowMaterial material, SystemVersionLevel versionLevel)
        {
            switch (versionLevel)
            {
                case SystemVersionLevel.Windows_11_After:
                    break;
                case SystemVersionLevel.Windows_11_21H2:
                case SystemVersionLevel.Windows_10_19H1:
                case SystemVersionLevel.Windows_10_Redstone_4:
                    switch (material)
                    {
                        case WindowMaterial.Mica:
                        case WindowMaterial.MicaAlt:
                            return WindowMaterial.Acrylic;
                        default:
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_10:
                    switch (material)
                    {
                        case WindowMaterial.Mica:
                        case WindowMaterial.MicaAlt:
                        case WindowMaterial.Acrylic:
                            return WindowMaterial.Gaussian;
                        default:
                            break;
                    }
                    break;
                case SystemVersionLevel.Windows_8:
                case SystemVersionLevel.Windows_7:
                    switch (material)
                    {
                        case WindowMaterial.Mica:
                        case WindowMaterial.MicaAlt:
                        case WindowMaterial.Acrylic:
                        case WindowMaterial.Gaussian:
                            return WindowMaterial.Integrated;
                        default:
                            break;
                    }
                    break;
            }
            return material;
        }
    }
}
