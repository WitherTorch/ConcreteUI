using System.Diagnostics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    public abstract partial class ButtonBase : DisposableUIElementBase, IMouseInteractHandler, IMouseMoveHandler
    {
        private ButtonTriState _pressState;
        private uint _version;
        private bool _enabled, _isPressed;

        public ButtonBase(IRenderer renderer, string themePrefix) : base(renderer, themePrefix)
        {
            _enabled = true;
            _pressState = (uint)ButtonTriState.None;
        }

        public override void OnSizeChanged() => Update();

        void IMouseMoveHandler.OnMouseMove(in MouseEventArgs args)
        {
            ButtonTriState pressState;
            if (_enabled && args.IsInSpecificSize(Size))
                pressState = _isPressed ? ButtonTriState.Pressed : ButtonTriState.Hovered;
            else
                pressState = ButtonTriState.None;
            if (ReferenceHelper.Exchange(ref _pressState, pressState) != pressState)
            {
                _version++;
                Update();
            }
        }

        void IMouseInteractHandler.OnMouseDown(ref HandleableMouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;
            args.Handle();
            _isPressed = true;
            if (ReferenceHelper.Exchange(ref _pressState, ButtonTriState.Pressed) != ButtonTriState.Pressed)
            {
                _version++;
                Update();
            }
        }

        void IMouseInteractHandler.OnMouseUp(in MouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagOptimized(MouseButtons.LeftButton))
                return;

            _isPressed = false;
            if (PressState != ButtonTriState.Pressed)
                return;

            if (ReferenceHelper.Exchange(ref _pressState, ButtonTriState.Hovered) != ButtonTriState.Hovered)
            {
                _version++;
                Update();
            }
            OnClick(in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnClick(in MouseEventArgs args) => Click?.Invoke(this, args);
    }
}
