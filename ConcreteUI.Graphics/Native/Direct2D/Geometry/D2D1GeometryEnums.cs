using System;

namespace ConcreteUI.Graphics.Native.Direct2D.Geometry
{
    /// <summary>
    /// Specifies how the intersecting areas of geometries or figures are combined to
    /// form the area of the composite geometry.
    /// </summary>
    public enum D2D1FillMode : uint
    {
        Alternate = 0,
        Winding = 1
    }

    /// <summary>
    /// Indicates whether the given segment should be stroked, or, if the join between
    /// this segment and the previous one should be smooth.
    /// </summary>
    [Flags]
    public enum D2D1PathSegment : uint
    {
        None = 0x00000000,
        ForceUnstroked = 0x00000001,
        ForceRoundLineJoin = 0x00000002,
    }

    /// <summary>
    /// Indicates whether the given figure is filled or hollow.
    /// </summary>
    public enum D2D1FigureBegin : uint
    {
        Filled = 0,
        Hollow = 1
    }

    /// <summary>
    /// Indicates whether the figure is open or closed on its end point.
    /// </summary>
    public enum D2D1FigureEnd : uint
    {
        Open = 0,
        Closed = 1
    }

    /// <summary>
    /// Defines the direction that an elliptical arc is drawn.
    /// </summary>
    public enum D2D1SweepDirection : uint
    {
        CounterClockwise = 0,
        Clockwise = 1
    }

    /// <summary>
    /// Differentiates which of the two possible arcs could match the given arc
    /// parameters.
    /// </summary>
    public enum D2D1ArcSize : uint
    {
        Small = 0,
        Large = 1
    }

    /// <summary>
    /// Describes how one geometry object is spatially related to another geometry
    /// object.
    /// </summary>
    public enum D2D1GeometryRelation : uint
    {
        /// <summary>
        /// The relation between the geometries couldn't be determined. This value is never
        /// returned by any D2D method.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The two geometries do not intersect at all.
        /// </summary>
        Disjoint = 1,
        /// <summary>
        /// The passed in geometry is entirely contained by the object.
        /// </summary>
        IsContained = 2,
        /// <summary>
        /// The object entirely contains the passed in geometry.
        /// </summary>
        Contains = 3,
        /// <summary>
        /// The two geometries overlap but neither completely contains the other.
        /// </summary>
        Overlap = 4,
    }

    /// <summary>
    /// Specifies how simple the output of a simplified geometry sink should be.
    /// </summary>
    public enum D2D1GeometrySimplificationOption
    {
        CubicsAndLines = 0,
        Lines = 1
    }
}