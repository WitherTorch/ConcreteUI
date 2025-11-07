using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class ComboBox
    {
        private sealed class AutoHeightVariable : LayoutVariable
        {
            private readonly WeakReference<ComboBox> _reference;

            public AutoHeightVariable(ComboBox element)
            {
                _reference = new WeakReference<ComboBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out ComboBox? element))
                    return 0;
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
