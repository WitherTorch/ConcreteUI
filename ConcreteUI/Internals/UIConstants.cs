﻿using System.Drawing;

namespace ConcreteUI.Internals
{
    internal static class UIConstants
    {
        public const double SquareRoot1Over2 = 0.7071067811865476;
        public const double SquareRoot3Over4 = 0.866025403784439;
        public const double SquareRoot1Over3 = 0.577350269189626;
        public const float SquareRoot1Over2_Single = (float)SquareRoot1Over2;
        public const float SquareRoot3Over4_Single = (float)SquareRoot3Over4;
        public const float SquareRoot1Over3_Single = (float)SquareRoot1Over3;

        public const float TitleFontSize = 11.5f;
        public const float MenuFontSize = 16f;
        public const float DefaultFontSize = 14f;
        public const float DefaultFontSizeInPoints = DefaultFontSize * FontSizeDipToPointsMultiplier;
        public const float FontSizeDipToPointsMultiplier = 72f / 96f;
        public const float DescriptionFontSize = 12f;
        public const float WizardWindowTitleFontSize = 22f;
        public const float WizardWindowTitleDescriptionFontSize = 14f;

        public const int ElementMarginDouble = ConcreteConstants.ElementMargin * 2;
        public const int ElementMarginHalf = ConcreteConstants.ElementMargin / 2;
        public const int WizardLeftPadding = 22;
        public const int WizardPadding = ConcreteConstants.ElementMargin;
        public const int WizardSubtitleLeftMargin = ConcreteConstants.ElementMargin;
        public const int WizardSubtitleMargin = ElementMarginHalf;
        public const float TitleBarButtonSizeHeight = 26;
        public const float TitleBarButtonSizeWidth = 36;
        public const float TitleBarIconSizeHeight = 10;
        public const float TitleBarIconSizeWidth = 10;
        public const int ScrollBarWidth = 14;
        public const int ScrollBarButtonWidth = ScrollBarWidth - 4;
        public const int ScrollBarScrollButtonWidth = ScrollBarWidth - 6;

        public static readonly SizeF TitleBarButtonSize = new SizeF(TitleBarButtonSizeWidth, TitleBarButtonSizeHeight);
        public static readonly SizeF TitleBarIconSize = new SizeF(TitleBarIconSizeWidth, TitleBarIconSizeHeight);
        public static readonly SizeF ScrollBarScrollButtonSize = new SizeF(ScrollBarScrollButtonWidth, ScrollBarScrollButtonWidth);
    }
}
