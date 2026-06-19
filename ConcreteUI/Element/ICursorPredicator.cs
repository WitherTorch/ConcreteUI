using ConcreteUI.Utils;

namespace ConcreteUI.Element;

public interface ICursorPredicator
{
    SystemCursorType? PredicatedCursor { get; }
}
