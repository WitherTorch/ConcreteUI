namespace ShioUI;

public enum TextAlignment
{
    TopLeft, TopCenter, TopRight,
    MiddleLeft, MiddleCenter, MiddleRight,
    BottomLeft, BottomCenter, BottomRight
}

public enum LayoutProperty : uint
{
    //Direct Properties
    Left,
    Top,
    Right,
    Bottom,
    //Indirect Properties (Need some extra checking for calculating layout)
    Width,
    Height,
    //Special Uses Only
    _Last,
    _DirectlyLast = Height,
    X = Left,
    Y = Top,
}
