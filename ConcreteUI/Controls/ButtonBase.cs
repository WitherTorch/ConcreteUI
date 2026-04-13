using System.Diagnostics;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Threading;

namespace ConcreteUI.Controls
{
    public abstract partial class ButtonBase : DisposableUIElementBase, IMouseInteractHandler, IMouseMoveHandler
    {
        private nuint _version;
        private uint _pressState;
        private bool _enabled, _isPressed;

        public ButtonBase(IElementContainer parent, string themePrefix) : base(parent, themePrefix)
        {
            _enabled = true;
            _pressState = (uint)ButtonTriState.None;
        }

        public override void OnSizeChanged() => Update();

        void IMouseMoveHandler.OnMouseMove(in MouseEventArgs args)
        {
            uint pressState;
            if (_enabled && args.IsInSpecificSize(Size))
                pressState = _isPressed ? (uint)ButtonTriState.Pressed : (uint)ButtonTriState.Hovered;
            else
                pressState = (uint)ButtonTriState.None;
            if (ReferenceHelper.Exchange(ref _pressState, pressState) != pressState)
            {
                OptimisticLock.Increase(ref _version);
                Update();
            }
        }

        void IMouseInteractHandler.OnMouseDown(ref HandleableMouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;
            args.Handle();
            _isPressed = true;
            if (ReferenceHelper.Exchange(ref _pressState, (uint)ButtonTriState.Pressed) != (uint)ButtonTriState.Pressed)
            {
                OptimisticLock.Increase(ref _version);
                Update();
            }
        }

        void IMouseInteractHandler.OnMouseUp(in MouseEventArgs args)
        {
            if (!_enabled || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
                return;

            _isPressed = false;
            if (PressState != ButtonTriState.Pressed)
                return;

            if (ReferenceHelper.Exchange(ref _pressState, (uint)ButtonTriState.Hovered) != (uint)ButtonTriState.Hovered)
            {
                OptimisticLock.Increase(ref _version);
                Update();
            }
            OnClick(in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnClick(in MouseEventArgs args) => Click?.Invoke(this, args);
    }
}
