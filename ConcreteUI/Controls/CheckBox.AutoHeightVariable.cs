using System;

using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private sealed class AutoHeightVariable : UIElementDependedVariable<CheckBox>
        {
            public AutoHeightVariable(CheckBox element) : base(element) { }

            protected override int Compute(CheckBox element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
