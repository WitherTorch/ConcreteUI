using System.Windows.Forms;

namespace ConcreteUI.Controls
{
    public interface IKeyEvents
    {
        void OnKeyDown(KeyEventArgs args);
        void OnKeyUp(KeyEventArgs args);
    }
}
