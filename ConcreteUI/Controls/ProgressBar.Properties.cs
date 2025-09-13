using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class ProgressBar
    {
        public double Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            set
            {
                double oldValue = _value;
                if (oldValue == value)
                    return;
                value = MathHelper.Clamp(value, 0.0, _maximium);
                if (oldValue == value)
                    return;
                _value = value;
                Update();
            }
        }

        public double Maximium
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maximium;
            set
            {
                if (_maximium == value || value <= 0.0)
                    return;
                _maximium = value;
                Update();
            }
        }

    }
}
