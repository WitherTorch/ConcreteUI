using System.Collections.Generic;
using System.Drawing;

namespace ConcreteUI.Layout;

public interface ILayoutEngine
{
    void RecalculateLayout(Size pageSize, UIElement? element);

    void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements) where TEnumerable : IEnumerable<UIElement?>;
}