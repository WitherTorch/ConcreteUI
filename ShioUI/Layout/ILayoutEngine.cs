using System.Collections.Generic;
using System.Drawing;

namespace ShioUI.Layout;

public interface ILayoutEngine
{
    void RecalculateLayout(Size pageSize, UIElement? element, ulong timestamp);

    void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, ulong timestamp) where TEnumerable : IEnumerable<UIElement?>;
}