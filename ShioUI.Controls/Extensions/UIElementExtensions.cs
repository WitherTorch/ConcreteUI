using System.Runtime.CompilerServices;

namespace ShioUI.Controls.Extensions;

public static class UIElementExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WithAutoWidth<T>(this T _this) where T : UIElement, IAutoWidthElement
    {
        _this.WidthExpression = _this.AutoWidthDefinition;
        return _this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WithAutoHeight<T>(this T _this) where T : UIElement, IAutoHeightElement
    {
        _this.HeightExpression = _this.AutoHeightDefinition;
        return _this;
    }
}
