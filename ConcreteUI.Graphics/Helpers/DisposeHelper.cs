using System;

using InlineMethod;

namespace ConcreteUI.Graphics.Helpers
{
    internal static class DisposeHelper
    {
        [Inline(InlineBehavior.Remove)]
        public static void DisposeAndSet<T>(ref T location, T newValue = default) where T : IDisposable
        {
            T oldValue;
            (oldValue, location) = (location, newValue);
            oldValue?.Dispose();
        }
    }
}
