using System.Runtime.InteropServices;

namespace ConcreteUI.Graphics.Native.DirectWrite
{
    /// <summary>
    /// The <see cref="DWriteTextRange"/> structure specifies a range of text positions where format is applied.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteTextRange
    {
        /// <summary>
        /// The start text position of the range.
        /// </summary>
        public int StartPosition;

        /// <summary>
        /// The number of text positions in the range.
        /// </summary>
        public int Length;

        public DWriteTextRange(int startPosition, int length)
        {
            StartPosition = startPosition;
            Length = length;
        }
    }

    /// <summary>
    /// Overall metrics associated with text after layout.<br/>
    /// All coordinates are in device independent pixels (DIPs).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteTextMetrics
    {
        /// <summary>
        /// Left-most point of formatted text relative to layout box
        /// (excluding any glyph overhang).
        /// </summary>
        public float Left;

        /// <summary>
        /// Top-most point of formatted text relative to layout box
        /// (excluding any glyph overhang).
        /// </summary>
        public float Top;

        /// <summary>
        /// The width of the formatted text ignoring trailing whitespace
        /// at the end of each line.
        /// </summary>
        public float Width;

        /// <summary>
        /// The width of the formatted text taking into account the
        /// trailing whitespace at the end of each line.
        /// </summary>
        public float WidthIncludingTrailingWhitespace;

        /// <summary>
        /// The height of the formatted text. The height of an empty string
        /// is determined by the size of the default font's line height.
        /// </summary>
        public float Height;

        /// <summary>
        /// Initial width given to the layout. Depending on whether the text
        /// was wrapped or not, it can be either larger or smaller than the
        /// text content width.
        /// </summary>
        public float LayoutWidth;

        /// <summary>
        /// Initial height given to the layout. Depending on the length of the
        /// text, it may be larger or smaller than the text content height.
        /// </summary>
        public float LayoutHeight;

        /// <summary>
        /// The maximum reordering count of any line of text, used
        /// to calculate the most number of hit-testing boxes needed.<br/>
        /// If the layout has no bidirectional text or no text at all,
        /// the minimum level is 1.
        /// </summary>
        public uint MaxBidiReorderingDepth;

        /// <summary>
        /// Total number of lines.
        /// </summary>
        public uint LineCount;
    }

    /// <summary>
    /// The <see cref="DWriteLineMetrics"> structure contains information about a formatted
    /// line of text.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteLineMetrics
    {
        /// <summary>
        /// The number of total text positions in the line.
        /// This includes any trailing whitespace and newline characters.
        /// </summary>
        public uint Length;

        /// <summary>
        /// The number of whitespace positions at the end of the line.  Newline
        /// sequences are considered whitespace.
        /// </summary>
        public uint TrailingWhitespaceLength;

        /// <summary>
        /// The number of characters in the newline sequence at the end of the line.
        /// If the count is zero, then the line was either wrapped or it is the
        /// end of the text.
        /// </summary>
        public uint NewlineLength;

        /// <summary>
        /// Height of the line as measured from top to bottom.
        /// </summary>
        public float Height;

        /// <summary>
        /// Distance from the top of the line to its baseline.
        /// </summary>
        public float Baseline;

        /// <summary>
        /// The line is trimmed.
        /// </summary>
        public bool IsTrimmed;
    }

    /// <summary>
    /// Geometry enclosing of text positions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteHitTestMetrics
    {
        /// <summary>
        /// First text position within the geometry.
        /// </summary>
        public uint TextPosition;

        /// <summary>
        /// Number of text positions within the geometry.
        /// </summary>
        public uint Length;

        /// <summary>
        /// Left position of the top-left coordinate of the geometry.
        /// </summary>
        public float Left;

        /// <summary>
        /// Top position of the top-left coordinate of the geometry.
        /// </summary>
        public float Top;

        /// <summary>
        /// Geometry's width.
        /// </summary>
        public float Width;

        /// <summary>
        /// Geometry's height.
        /// </summary>
        public float Height;

        /// <summary>
        /// Bidi level of text positions enclosed within the geometry.
        /// </summary>
        public uint BidiLevel;

        /// <summary>
        /// Geometry encloses text?
        /// </summary>
        public bool IsText;

        /// <summary>
        /// Range is trimmed.
        /// </summary>
        public bool IsTrimmed;
    }
}
