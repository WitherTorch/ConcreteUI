using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class ProgressBar
    {
        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            set
            {
                float oldValue = _value;
                if (oldValue == value)
                    return;
                value = MathHelper.Clamp(value, 0.0f, _maximium);
                if (oldValue == value)
                    return;
                _value = value;
                Update();
            }
        }

        public float Maximium
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maximium;
            set
            {
                if (_maximium == value || value <= 0)
                    return;
                _maximium = value;
                Update();
            }
        }

    }
}
