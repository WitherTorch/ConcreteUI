using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class Button
    {
        private sealed class AutoHeightVariable : LayoutVariable
        {
            private readonly WeakReference<Button> _reference;

            public AutoHeightVariable(Button button)
            {
                _reference = new WeakReference<Button>(button);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out Button? element))
                    return 0;
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
