using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Controls
{
    public abstract class PopupElementBase : UIElement, IMouseNotifyEvents
    {
        private readonly CoreWindow _window;

        protected PopupElementBase(CoreWindow window, string themePrefix) : base(window, themePrefix)
        {
            _window = window;
        }

        public void Close() => _window.CloseOverlayElement(this);

        public virtual void OnMouseDown(in MouseNotifyEventArgs args) { }

        public virtual void OnMouseUp(in MouseNotifyEventArgs args)
        {
            if (!Bounds.Contains(args.Location))
                Close();
        }

        public virtual void OnMouseMove(in MouseNotifyEventArgs args) { }
    }
}
