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

        public virtual void OnMouseDown(in MouseEventArgs args) { }

        public virtual void OnMouseUp(in MouseEventArgs args)
        {
            if (!Bounds.Contains(args.Location))
                Close();
        }

        public virtual void OnMouseMove(in MouseEventArgs args) { }
    }
}
