using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class CheckBox
    {
        private sealed class AutoWidthVariable : LayoutVariable
        {
            private readonly WeakReference<CheckBox> _reference;

            public AutoWidthVariable(CheckBox element)
            {
                _reference = new WeakReference<CheckBox>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out CheckBox? element))
                    return 0;
                using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize);
                return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + manager.GetComputedValue(element, LayoutProperty.Height) + UIConstants.ElementMargin;
            }
        }
    }
}
