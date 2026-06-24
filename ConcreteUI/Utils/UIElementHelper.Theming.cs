using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Theme;

using WitherTorch.Common.Buffers;
using WitherTorch.Common.Extensions;
using WitherTorch.Common.Helpers;

namespace ConcreteUI.Utils;

partial class UIElementHelper
{
    public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
    {
        int length = brushes.Length;
        if (length != nodes.Length)
            throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
        if (length <= 0)
            return;
        ApplyThemeUnsafe(provider, brushes, nodes, (nuint)length);
    }

    public static void ApplyTheme(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix)
    {
        int length = brushes.Length;
        if (length != nodes.Length)
            throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
        if (length <= 0)
            return;
        ApplyThemeUnsafe(provider, brushes, nodes, nodePrefix, (nuint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, nuint length)
    {
        ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
        ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
        for (nuint i = 0; i < length; i++)
            ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix, nuint length)
    {
        ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
        ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
        for (nuint i = 0; i < length; i++)
            ApplyTheme(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyTheme(IThemeResourceProvider provider, ref D2D1Brush? brushRef, string node)
        => DisposeHelper.SwapDispose(ref brushRef, provider.TryGetBrush(node, out D2D1Brush? result) ? result.Clone() : null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyTheme(IThemeResourceProvider provider, UIElement? element)
        => element?.ApplyTheme(provider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyTheme<TEnumerable>(IThemeResourceProvider provider, TEnumerable elements)
        where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        ApplyThemeCore(provider, in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyThemeCore(IThemeResourceProvider provider, ref readonly UIElement? elementArrayRef, nuint length)
    {
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i)?.ApplyTheme(provider);
            UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1)?.ApplyTheme(provider);
            UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2)?.ApplyTheme(provider);
            UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 3)?.ApplyTheme(provider);
        }
        switch (length)
        {
            case 3:
                UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2)?.ApplyTheme(provider);
                goto case 2;
            case 2:
                UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1)?.ApplyTheme(provider);
                goto case 1;
            case 1:
                UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i)?.ApplyTheme(provider);
                break;
        }
    }
}
