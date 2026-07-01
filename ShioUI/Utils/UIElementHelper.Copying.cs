using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UIElement?[] CopyElementsToArray<TEnumerable>(TEnumerable elements)
        where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        int count = scope.Count;
        if (count <= 0)
            return Array.Empty<UIElement?>();
        UIElement?[] result = new UIElement?[count];
        CopyElementsToArrayCore(ref UnsafeHelper.GetArrayDataReference(result), in scope.GetReferenceOfFirstElement(), (nuint)scope.Count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UIElement?[] CopyElementsToArrayUnsafe(ref readonly UIElement? elementsRef, int count)
    {
        if (count <= 0)
            return Array.Empty<UIElement?>();
        UIElement?[] result = new UIElement?[count];
        CopyElementsToArrayCore(ref UnsafeHelper.GetArrayDataReference(result), in elementsRef, MathHelper.MakeUnsigned(count));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyElementsToArrayCore(ref UIElement? destination, ref readonly UIElement? sourceArrayRef, nuint length)
    {
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            SetValueWithOffset(ref destination, in sourceArrayRef, i);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 1);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 2);
            SetValueWithOffset(ref destination, in sourceArrayRef, i + 3);
        }
        switch (length)
        {
            case 3:
                SetValueWithOffset(ref destination, in sourceArrayRef, i + 2);
                goto case 2;
            case 2:
                SetValueWithOffset(ref destination, in sourceArrayRef, i + 1);
                goto case 1;
            case 1:
                SetValueWithOffset(ref destination, in sourceArrayRef, i);
                break;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SetValueWithOffset(ref UIElement? destination, ref readonly UIElement? sourceArrayRef, int offset)
            => UnsafeHelper.AddTypedOffset(ref destination, offset) = UnsafeHelper.AddTypedOffsetAsReadOnly(in sourceArrayRef, offset);
    }
}
