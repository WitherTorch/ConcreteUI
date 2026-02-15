using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Internals.Native;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Internals
{
    internal static class ClearTypeSwitcher
    {
        private static readonly ConditionalWeakTable<D2D1DeviceContext, StrongBox<bool>>? _stateDict;

        unsafe static ClearTypeSwitcher()
        {
            const uint SPI_GETCLEARTYPE = 0x1048;
            SysBool enabled;
            if (User32.SystemParametersInfoW(SPI_GETCLEARTYPE, 0, &enabled, 0) && enabled)
                _stateDict = new ConditionalWeakTable<D2D1DeviceContext, StrongBox<bool>>();
            else
                _stateDict = null;
        }

        public static bool IsEnabled => _stateDict is not null;

        public static bool SetClearType(D2D1DeviceContext context, bool enable)
        {
            ConditionalWeakTable<D2D1DeviceContext, StrongBox<bool>>? dict = _stateDict;
            if (dict is null)
                return false;
            StrongBox<bool> boxedState = dict.GetValue(context, static _ => new StrongBox<bool>(false));
            if (enable)
            {
                lock (boxedState)
                {
                    if (boxedState.Value)
                        return false;
                    context.TextAntialiasMode =  D2D1TextAntialiasMode.ClearType;
                    boxedState.Value = true;
                }
            }
            else
            {
                lock (boxedState)
                {
                    if (!boxedState.Value)
                        return false;
                    context.TextAntialiasMode = D2D1TextAntialiasMode.Grayscale;
                    boxedState.Value = false;
                }
            }
            return true;
        }
    }
}
