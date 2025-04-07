using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;

namespace ConcreteUI.Theme
{
    public interface IThemedElementTemplate
    {
        public string ElementClassName { get; }

        public string[] ThemedBrushNames { get; }

        public void ApplyThemedBrushes(IReadOnlyDictionary<string, D2D1Brush> brushes);
    }
}
