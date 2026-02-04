namespace ConcreteUI.Internals.Native
{
    internal enum DwmWindowAttribute : uint
    {
        NCRenderingEnabled = 1,
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

    internal enum DwmSystemBackdropType : uint
    {
        Auto,
        None,
        MainWindow,
        TransientWindow,
        TabbedWindow
    }

    internal enum DwmWindowCornerPreference : uint
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    internal enum DwmBlurBehindFlags : uint
    {
        None = 0,
        Enable = 1,
        BlurRegion = 2,
        TransitionMaximized = 4
    }

    internal enum DwmNCRenderingPolicy : uint
    {
        /// <summary>
        /// Enable/disable non-client rendering based on window style
        /// </summary>
        UseWindowStyle,
        /// <summary>
        /// Disabled non-client rendering; window style is ignored
        /// </summary>
        Disabled,
        /// <summary>
        /// Enabled non-client rendering; window style is ignored
        /// </summary>
        Enabled,
        _Last
    }
}
