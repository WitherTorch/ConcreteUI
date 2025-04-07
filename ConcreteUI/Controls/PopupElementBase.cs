using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Controls
{
    public abstract class PopupElementBase : UIElement, IGlobalMouseEvents
    {
        private readonly CoreWindow _window;

        protected PopupElementBase(CoreWindow window) : base(window)
        {
            _window = window;
        }

        public void Close() => _window.CloseOverlayElement(this);

        public virtual void OnMouseDown(in MouseInteractEventArgs args) { }

        public virtual void OnMouseUp(in MouseInteractEventArgs args)
        {
            if (!Bounds.Contains(args.Location))
                Close();
        }

        public virtual void OnMouseMove(in MouseInteractEventArgs args) { }
    }
}
