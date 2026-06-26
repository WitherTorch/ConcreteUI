using System.Collections.Generic;

using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

public interface IThemedElementTemplate
{
    public string ElementClassName { get; }

    public string[] ThemedBrushNames { get; }

    public void ApplyThemedBrushes(IReadOnlyDictionary<string, D2D1Brush> brushes);
}
