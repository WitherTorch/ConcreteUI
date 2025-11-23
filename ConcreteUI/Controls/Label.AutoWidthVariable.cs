using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class Label
    {
        private sealed class AutoWidthVariable : LayoutVariable
        {
            private readonly WeakReference<Label> _reference;

            public AutoWidthVariable(Label element)
            {
                _reference = new WeakReference<Label>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out Label? element))
                    return 0;
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text,
                    fontName, element._alignment, element._fontSize);
                return MathI.Ceiling(layout.GetMetrics().Width);
            }
        }
    }
}
