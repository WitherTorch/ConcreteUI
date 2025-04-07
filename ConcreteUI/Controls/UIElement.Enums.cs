namespace ConcreteUI.Controls
{
    public enum TextAlignment
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    public enum LayoutProperty
    {
        None = -1,
        //Direct Properties
        Left,
        Top,
        Right,
        Bottom,
        //Indirect Properties (Need some extra checking for calculating layout)
        Height,
        Width,
        //Special Uses Only
        _Last,
        _DirectlyLast = Height,
        X = Left,
        Y = Top,
    }
}
