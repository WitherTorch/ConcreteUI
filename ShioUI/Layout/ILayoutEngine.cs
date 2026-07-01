using System.Collections.Generic;
using System.Drawing;

using ShioUI.Utils;

namespace ShioUI.Layout;

// Just a specification for LayoutEngine and LayoutEngineRentScope, don't implement it manually!
public interface ILayoutEngine
{
    void RecalculateLayout(Size pageSize, UIElement? element, in RecalculateLayoutInformation information);

    void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, in RecalculateLayoutInformation information) where TEnumerable : IEnumerable<UIElement?>;

    void RecalculateLayoutUnsafe(Size pageSize, ref readonly UIElement? elementsRef, int count, in RecalculateLayoutInformation information);
}