using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Internals;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class TextBox
    {
        private sealed class AutoHeightVariable : LayoutVariable
        {
            private readonly WeakReference<TextBox> _reference;

            public AutoHeightVariable(TextBox element)
            {
                _reference = new WeakReference<TextBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out TextBox? element))
                    return 0;
                return MathI.Ceiling(ComputeCore(element, manager)) + UIConstants.ElementMargin;
            }

            private static float ComputeCore(TextBox element, in LayoutVariableManager manager)
            {
                string? fontName = element._fontName;
                if (fontName is null)
                    return 0;
                float fontSize = element._fontSize;
                if (element._multiLine)
                {
                    using DWriteTextLayout layout = TextFormatHelper.CreateTextLayout(element._text, fontName, element._alignment, fontSize);
                    element.SetRenderingPropertiesForMultiLine(layout, manager.GetComputedValue(element, LayoutProperty.Width) - UIConstants.ElementMargin,
                        element.Renderer.GetPointsPerPixel());
                    return layout.GetMetrics().Height;
                }
                return FontHeightHelper.GetFontHeight(fontName, fontSize);
            }
        }
    }
}
