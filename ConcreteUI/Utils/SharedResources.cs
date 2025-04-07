using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.DirectWrite;

namespace ConcreteUI.Utils
{
    public static class SharedResources
    {
        private static readonly DWriteFactory _writeFactory = DWriteFactory.Create(DWriteFactoryType.Shared);

        public static DWriteFactory DWriteFactory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _writeFactory;
        }
    }
}
