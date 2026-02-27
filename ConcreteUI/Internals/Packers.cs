using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConcreteUI.Internals
{
    [StructLayout(LayoutKind.Explicit, Size = sizeof(ulong))]
    internal readonly ref struct UInt64Packer
    {
        [FieldOffset(0)] public readonly Point PointValue;
        [FieldOffset(0)] public readonly Size SizeValue;
        [FieldOffset(0)] public readonly ulong UInt64Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64Packer(in Point value) => PointValue = value;
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64Packer(in Size value) => SizeValue = value;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64Packer(in ulong value) => UInt64Value = value;
    }
}
