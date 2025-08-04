using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private sealed class AutoHeightVariable : LayoutVariable
        {
            private readonly WeakReference<CheckBox> _reference;

            public AutoHeightVariable(CheckBox element)
            {
                _reference = new WeakReference<CheckBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out CheckBox? element))
                    return 0;
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
