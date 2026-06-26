using ShioUI.Layout;

namespace ShioUI;

public static class UIConstants
{
    // Layout Constants
    public const int ElementMargin = 6;

    public const float TitleFontSize = 12f;
    public const float MenuFontSize = 16f;
    public const float DefaultFontSize = 14f;
    public const float BoxFontSize = 13f;
    public const float DescriptionFontSize = 12f;
    public const float WizardWindowTitleFontSize = 22f;
    public const float WizardWindowTitleDescriptionFontSize = 14f;

    public const int ElementMarginDouble = ElementMargin * 2;
    public const int ElementMarginHalf = ElementMargin / 2;

    // Layout Nodes
    public static readonly LayoutNode ElementMarginDefinition = LayoutNode.Fixed(ElementMargin);
    public static readonly LayoutNode ElementMarginDoubleDefinition = LayoutNode.Fixed(ElementMarginDouble);
    public static readonly LayoutNode ElementMarginHalfDefinition = LayoutNode.Fixed(ElementMarginHalf);
}