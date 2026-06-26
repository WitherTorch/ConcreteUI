using System.Drawing;

namespace ShioUI.Internals;

internal static class UIConstantsPrivate
{
    public const int WizardLeftPadding = 22;
    public const int WizardPadding = UIConstants.ElementMargin;
    public const int WizardSubtitleLeftMargin = UIConstants.ElementMargin;
    public const int WizardSubtitleMargin = UIConstants.ElementMarginHalf;
    public const int TitleBarButtonSizeHeight = 26;
    public const int TitleBarButtonSizeWidth = 36;
    public const int TitleBarIconSizeHeight = 10;
    public const int TitleBarIconSizeWidth = 10;

    public static readonly SizeF TitleBarButtonSize = new SizeF(TitleBarButtonSizeWidth, TitleBarButtonSizeHeight);
    public static readonly SizeF TitleBarIconSize = new SizeF(TitleBarIconSizeWidth, TitleBarIconSizeHeight);
}
