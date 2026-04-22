using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    partial class Button
    {
        private sealed class AutoHeightVariable : UIElementDependedVariable<Button>
        {
            public AutoHeightVariable(Button element) : base(element) { }

            protected override int Compute(Button element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
