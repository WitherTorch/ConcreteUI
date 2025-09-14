namespace ConcreteUI.Controls
{
    public readonly record struct DpiChangedEventArgs(uint Dpi, float PointsPerPixel, float PixelsPerPoint);

    internal interface IDpiAwareEvents
    {
        void OnDpiChanged(in DpiChangedEventArgs args);
    }
}
