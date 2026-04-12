using ConcreteUI.Utils;
using ConcreteUI.Window;

namespace ConcreteUI.Controls
{
    public abstract class PopupElementBase : UIElement, IGlobalMouseInteractHandler
    {
        protected PopupElementBase(IElementContainer parent, string themePrefix) : base(parent, themePrefix) { }

        public void Close() => Window.CloseOverlayElement(this);

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
