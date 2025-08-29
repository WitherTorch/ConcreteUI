using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    public interface ICursorPredicator
    {
        SystemCursorType? PredicatedCursor { get; }
    }
}
