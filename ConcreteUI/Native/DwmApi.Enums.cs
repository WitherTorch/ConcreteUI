namespace ConcreteUI.Native
{
    public enum DwmWindowAttribute : uint
    {
        NCRenderingEnabled,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        Caption_Button_Bounds,
        NonClientRTLLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation,
        PassiveUpdateMode,
        UseHostBackdropBrush,
        UseImmersiveDarkMode = 20,
        WindowCornerPreference = 33,
        BorderColor,
        CaptionColor,
        TextColor,
        VisibleFrameBorderThickness,
        SystemBackdropType
    }

    public enum DwmSystemBackdropType : uint
    {
        Auto,
        None,
        MainWindow,
        TransientWindow,
        TabbedWindow
    }

    public enum DwmWindowCornerPreference : uint
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    public enum DwmBlurBehindFlags : uint
    {
        None = 0,
        Enable = 1,
        BlurRegion = 2,
        TransitionMaximized = 4
    }
}
