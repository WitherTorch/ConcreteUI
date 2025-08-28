using ConcreteUI.Window2;

namespace ConcreteUI.Controls
{
    public interface ICursorPredicator
    {
        SystemCursorType? PredicatedCursor { get; }
    }
}
