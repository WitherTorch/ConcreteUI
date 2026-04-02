using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Controls
{
    public abstract class PopupElementBase : UIElement, IGlobalMouseInteractHandler
    {
        private readonly CoreWindow _window;

        protected PopupElementBase(CoreWindow window, string themePrefix) : base(window, themePrefix)
        {
            _window = window;
        }

        public void Close() => _window.CloseOverlayElement(this);

        protected virtual void OnMouseDownGlobally(in MouseEventArgs args) { }

        protected virtual void OnMouseUpGlobally(in MouseEventArgs args)
        {
            if (!Bounds.Contains(args.Location))
                Close();
        }

        void IGlobalMouseInteractHandler.OnMouseDownGlobally(in MouseEventArgs args) => OnMouseDownGlobally(in args);

        void IGlobalMouseInteractHandler.OnMouseUpGlobally(in MouseEventArgs args) => OnMouseUpGlobally(in args);
    }
}
