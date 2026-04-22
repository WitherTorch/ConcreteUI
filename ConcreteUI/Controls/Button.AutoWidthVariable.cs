using System;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

namespace ConcreteUI.Controls
{
    partial class Button
    {
        private sealed class AutoWidthVariable : UIElementDependedVariable<Button>
        {
            public AutoWidthVariable(Button element) : base(element) { }

            protected override int Compute(Button element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, element._fontSize);
                return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + UIConstants.ElementMarginDouble * 2;
            }
        }
    }
}
