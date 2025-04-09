using System.Windows.Forms;

namespace ConcreteUI.Controls
{
    public interface ICursorPredicator
    {
        Cursor? PredicatedCursor { get; }
    }
}
