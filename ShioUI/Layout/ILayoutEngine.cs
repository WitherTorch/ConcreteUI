using System.Collections.Generic;
using System.Drawing;

using ShioUI.Utils;

namespace ShioUI.Layout;

public interface ILayoutEngine
{
    void RecalculateLayout(Size pageSize, UIElement? element, in RecalculateLayoutInformation information);

    void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, in RecalculateLayoutInformation information) where TEnumerable : IEnumerable<UIElement?>;
}