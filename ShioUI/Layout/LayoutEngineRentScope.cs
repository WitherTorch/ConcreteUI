using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;

using ShioUI.Utils;

namespace ShioUI.Layout;

[StructLayout(LayoutKind.Auto)]
public ref struct LayoutEngineRentScope : ILayoutEngine, IDisposable
{
    private readonly Pool<LayoutEngine> _pool;
    private LayoutEngine? _engine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LayoutEngineRentScope(Pool<LayoutEngine> pool, LayoutEngine engine)
    {
        _pool = pool;
        _engine = engine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutEngineRentScope Rent(Pool<LayoutEngine> pool) => new LayoutEngineRentScope(pool, pool.Rent());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void RecalculateLayout(Size pageSize, UIElement? element, in RecalculateLayoutInformation information)
        => _engine!.RecalculateLayout(pageSize, element, information); // 如果 engine 等於 null 則正常擲出 NRE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void RecalculateLayout<TEnumerable>(Size pageSize, TEnumerable elements, in RecalculateLayoutInformation information) where TEnumerable : IEnumerable<UIElement?>
        => _engine!.RecalculateLayout(pageSize, elements, information); // 如果 engine 等於 null 則正常擲出 NRE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        LayoutEngine? engine = _engine;
        if (engine is null)
            return;
        _engine = null;
        _pool.Return(engine);
    }
}
