using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private sealed class AutoWidthVariable : UIElementDependedVariable<CheckBox>
        {
            public AutoWidthVariable(CheckBox element) : base(element) { }

            protected override int Compute(CheckBox element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, element._fontSize);
                return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + manager.GetComputedValue(element, LayoutProperty.Height) + UIConstants.ElementMargin;
            }
        }
    }
}
