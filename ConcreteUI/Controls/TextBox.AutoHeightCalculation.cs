﻿using System;

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
                return MathI.Ceiling(FontHeightHelper.GetFontHeight(NullSafetyHelper.ThrowIfNull(element._fontName), element._fontSize)) + UIConstants.ElementMargin;
            }
        }
    }
}
