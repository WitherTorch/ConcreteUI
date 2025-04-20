using System.Runtime.CompilerServices;

namespace ConcreteUI
{
    public static class ConcreteSettings
    {
        public const string ReservedGpuName_None = "#none";
        public const string ReservedGpuName_Default = "#default";
        public const string ReservedGpuName_MinimumPower = "#default_minimum_power";
        public const string ReservedGpuName_HighPerformance = "#default_high_performance";

        private static WindowMaterial _windowMaterial = WindowMaterial.Default;

        public static bool UseDebugMode { get; set; } = false;
        public static string TargetGpuName { get; set; } = ReservedGpuName_Default;
        public static WindowMaterial WindowMaterial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _windowMaterial;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _windowMaterial = value;
        }
    }
}
