using System.Runtime.CompilerServices;

using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    public abstract partial class ButtonBase : UIElement, IMouseEvents
    {
        public event MouseInteractEventHandler? Click;

        private bool _enabled;
        private ButtonPressState _pressState;
        private bool previousMouseEnterState;

        public ButtonBase(IRenderer renderer, string themePrefix) : base(renderer, themePrefix)
        {
            _enabled = true;
            _pressState = ButtonPressState.Default;
            previousMouseEnterState = false;
        }

        public override void OnSizeChanged() => Update();

        public void OnMouseDown(in MouseInteractEventArgs args)
        {
            if (!_enabled || ((args.Keys & MouseKeys.LeftButton) != MouseKeys.LeftButton))
                return;
            _pressState = ButtonPressState.Pressed;
            Update();
        }

        public void OnMouseMove(in MouseInteractEventArgs args)
        {
            bool mouseEnterState = _enabled && Bounds.Contains(args.Location);
            if (mouseEnterState)
            {
                if (_pressState == ButtonPressState.Default)
                    _pressState = ButtonPressState.Hovered;
            }
            else
            {
                _pressState = ButtonPressState.Default;
            }
            if (previousMouseEnterState ^ mouseEnterState)
            {
                previousMouseEnterState = mouseEnterState;
                Update();
            }
        }

        public void OnMouseUp(in MouseInteractEventArgs args)
        {
            if (_enabled)
            {
                ButtonPressState previousButtonType = _pressState;
                if (Bounds.Contains(args.Location))
                {
                    _pressState = ButtonPressState.Hovered;
                }
                else
                {
                    _pressState = ButtonPressState.Default;
                }
                Update();
                if (previousButtonType == ButtonPressState.Pressed) OnClick(args);
            }
            else
            {
                if (_pressState != ButtonPressState.Default)
                {
                    _pressState = ButtonPressState.Default;
                    Update();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnClick(in MouseInteractEventArgs args) => Click?.Invoke(this, args);
    }
}
