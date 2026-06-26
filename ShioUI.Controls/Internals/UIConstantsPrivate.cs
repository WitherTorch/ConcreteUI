using System.Drawing;

namespace ShioUI.Controls.Internals;

internal static class UIConstantsPrivate
{
    public const int ScrollBarWidth = 14;
    public const int ScrollBarButtonWidth = ScrollBarWidth - 4;
    public const int ScrollBarScrollButtonWidth = ScrollBarWidth - UIConstants.ElementMargin;

    public static readonly SizeF ScrollBarScrollButtonSize = new SizeF(ScrollBarScrollButtonWidth, ScrollBarScrollButtonWidth);
}
