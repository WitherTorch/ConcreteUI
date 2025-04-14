using System.Drawing;

namespace ConcreteUI.Internals
{
    internal static class UIConstantsPrivate
    {
        public const double SquareRoot1Over2 = 0.7071067811865476;
        public const double SquareRoot3Over4 = 0.866025403784439;
        public const double SquareRoot1Over3 = 0.577350269189626;
        public const float SquareRoot1Over2_Single = (float)SquareRoot1Over2;
        public const float SquareRoot3Over4_Single = (float)SquareRoot3Over4;
        public const float SquareRoot1Over3_Single = (float)SquareRoot1Over3;

        public const int WizardLeftPadding = 22;
        public const int WizardPadding = UIConstants.ElementMargin;
        public const int WizardSubtitleLeftMargin = UIConstants.ElementMargin;
        public const int WizardSubtitleMargin = UIConstants.ElementMarginHalf;
        public const int ScrollBarWidth = 14;
        public const int ScrollBarButtonWidth = ScrollBarWidth - 4;
        public const int ScrollBarScrollButtonWidth = ScrollBarWidth - UIConstants.ElementMargin;
        public const float TitleBarButtonSizeHeight = 26;
        public const float TitleBarButtonSizeWidth = 36;
        public const float TitleBarIconSizeHeight = 10;
        public const float TitleBarIconSizeWidth = 10;

        public static readonly SizeF ScrollBarScrollButtonSize = new SizeF(ScrollBarScrollButtonWidth, ScrollBarScrollButtonWidth);
        public static readonly SizeF TitleBarButtonSize = new SizeF(TitleBarButtonSizeWidth, TitleBarButtonSizeHeight);
        public static readonly SizeF TitleBarIconSize = new SizeF(TitleBarIconSizeWidth, TitleBarIconSizeHeight);
    }
}
