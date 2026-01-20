using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

using WitherTorch.Common.Extensions;

namespace ConcreteUI.Controls
{
    public abstract partial class ButtonBase : DisposableUIElementBase, IMouseInteractEvents
    {
        private ButtonTriState _pressState;
        private bool _enabled, _isPressed;

        public ButtonBase(IRenderer renderer, string themePrefix) : base(renderer, themePrefix)
        {
            _enabled = true;
            _pressState = ButtonTriState.None;
        }

        public override void OnSizeChanged() => Update();

        public void OnMouseDown(ref MouseInteractEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;
            args.Handle();
            _pressState = ButtonTriState.Pressed;
            _isPressed = true;
            Update();
        }

        public void OnMouseMove(in MouseNotifyEventArgs args)
        {
            ButtonTriState pressState;
            if (_enabled && Bounds.Contains(args.Location))
                pressState = _isPressed ? ButtonTriState.Pressed : ButtonTriState.Hovered;
            else
                pressState = ButtonTriState.None;
            if (_pressState == pressState)
                return;
            _pressState = pressState;
            Update();
        }

        public void OnMouseUp(in MouseNotifyEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            _isPressed = false;
            if (_pressState != ButtonTriState.Pressed)
                return;

            OnClick(in args);
            _pressState = ButtonTriState.Hovered;
            Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnClick(in MouseNotifyEventArgs args) => Click?.Invoke(this, args);
    }
}
