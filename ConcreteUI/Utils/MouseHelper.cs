using System.Drawing;

using ConcreteUI.Native;

using LocalsInit;

namespace ConcreteUI.Utils
{
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
}
