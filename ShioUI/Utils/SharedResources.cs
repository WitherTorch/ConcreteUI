using System.Runtime.CompilerServices;

using ShioUI.Graphics.Native.DirectWrite;

namespace ShioUI.Utils;

public static class SharedResources
{
    private static readonly DWriteFactory _writeFactory = DWriteFactory.Create(DWriteFactoryType.Shared);

    public static DWriteFactory DWriteFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _writeFactory;
    }
}
