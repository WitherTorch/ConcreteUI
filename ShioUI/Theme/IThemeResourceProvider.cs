using System.Diagnostics.CodeAnalysis;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

public interface IThemeResourceProvider
{
    string FontName { get; }
    IThemeContext ThemeContext { get; }
    IThemeResourceProvider Clone();
    bool TryGetBrush(string node, [NotNullWhen(true)] out D2D1Brush? brush);
    bool TryGetColor(string node, out D2D1ColorF color);
}