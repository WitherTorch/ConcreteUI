using ConcreteUI.Utils;

namespace ConcreteUI.Input
{
    public interface IInputMethodHandler
    {
        void StartIMEComposition(InputMethod ime, InputMethodContext context);
        void OnIMEComposition(InputMethod ime, InputMethodContext context, string str, IMECompositionFlags flags, int cursorPosition);
        void OnIMECompositionResult(InputMethod ime, InputMethodContext context, string str, IMECompositionFlags flags);
        void EndIMEComposition(InputMethod ime, InputMethodContext context);
    }
}
