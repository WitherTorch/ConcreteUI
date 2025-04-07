using ConcreteUI.Native;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Input
{
    public interface IIMEControl
    {
        void StartIMEComposition();
        void OnIMEComposition(string str, IMECompositionFlags flags, int cursorPosition);
        void OnIMECompositionResult(string str, IMECompositionFlags flags);
        void EndIMEComposition();
        Rect GetInputArea();
    }
}
