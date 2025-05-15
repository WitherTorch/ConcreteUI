using System;

using ConcreteUI.Graphics.Native.DirectWrite;
using ConcreteUI.Layout;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Controls
{
    partial class Button
    {
        private sealed class AutoWidthVariable : LayoutVariable
        {
            private readonly WeakReference<Button> _reference;

            public AutoWidthVariable(Button element)
            {
                _reference = new WeakReference<Button>(element);
            }

            public override int Compute(in LayoutVariableManager manager)
            {
                if (!_reference.TryGetTarget(out Button? element))
                    return 0;
                using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize);
                return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + UIConstants.ElementMarginDouble * 2;
            }
        }
    }
}
