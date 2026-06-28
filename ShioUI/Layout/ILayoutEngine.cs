using System.Collections.Generic;
using System.Drawing;

namespace ShioUI.Layout;

public interface ILayoutEngine
{
    void RecalculateLayout(Size pageSize, UIElement? element, ulong timestamp, bool clearCache);

    void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, ulong timestamp, bool clearCache) where TEnumerable : IEnumerable<UIElement?>;
}