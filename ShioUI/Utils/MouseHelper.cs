using System.Drawing;

using LocalsInit;

using ShioUI.Internals.Native;

namespace ShioUI.Utils;

public static class MouseHelper
{
    [LocalsInit(false)]
    public static unsafe Point GetMousePosition()
    {
        Point point;
        if (User32.GetCursorPos(&point))
            return point;
        return Point.Empty;
    }
}
