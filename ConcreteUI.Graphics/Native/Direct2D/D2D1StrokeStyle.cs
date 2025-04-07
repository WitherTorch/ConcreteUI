using System;
using System.Security;

using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Resource interface that holds pen style properties.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public unsafe sealed class D2D1StrokeStyle : D2D1Resource
    {
        private static readonly Guid IID_D2D1StrokeStyle = new Guid(0x2cd906aa, 0x12e2, 0x11dc, 0x9f, 0xed, 0x00, 0x11, 0x43, 0xa0, 0x55, 0xf9);

        private new enum MethodTable
        {
            _Start = D2D1Resource.MethodTable._End,
            GetStartCap = _Start,
            GetEndCap,
            GetDashCap,
            GetMiterLimit,
            GetLineJoin,
            GetDashOffset,
            GetDashStyle,
            GetDashesCount,
            GetDashes,
            _End
        }

        public D2D1StrokeStyle(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1CapStyle StartCap => GetStartCap();

        public D2D1CapStyle EndCap => GetEndCap();

        public D2D1CapStyle DashCap => GetDashCap();

        public float MiterLimit => GetMiterLimit();

        public D2D1LineJoin LineJoin => GetLineJoin();

        public float DashOffset => GetDashOffset();

        public D2D1DashStyle DashStyle => GetDashStyle();

        [Inline(InlineBehavior.Remove)]
        private D2D1CapStyle GetStartCap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetStartCap);
            return ((delegate*<void*, D2D1CapStyle>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1CapStyle GetEndCap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetEndCap);
            return ((delegate*<void*, D2D1CapStyle>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1CapStyle GetDashCap()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDashCap);
            return ((delegate*<void*, D2D1CapStyle>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetMiterLimit()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMiterLimit);
            return ((delegate*<void*, float>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private D2D1LineJoin GetLineJoin()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetLineJoin);
            return ((delegate*<void*, D2D1LineJoin>)functionPointer)(nativePointer);
        }

        [Inline(InlineBehavior.Remove)]
        private float GetDashOffset()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDashOffset);
            return ((delegate*<void*, float>)functionPointer)(nativePointer);
        }

        private D2D1DashStyle GetDashStyle()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDashStyle);
            return ((delegate*<void*, D2D1DashStyle>)functionPointer)(nativePointer);
        }

        public uint GetDashesCount()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDashesCount);
            return ((delegate*<void*, uint>)functionPointer)(nativePointer);
        }

        /// <summary>
        /// Returns the dashes from the object into a new allocated array.
        /// </summary>
        /// <returns></returns>
        public float[] GetDashes()
        {
            uint count = GetDashesCount();
            float[] result = new float[count];
            fixed (float* ptr = result)
                GetDashes(ptr, count);
            return result;
        }

        /// <summary>
        /// Returns the dashes from the object into a user allocated array. The user must
        /// call <see cref="GetDashesCount"/> to retrieve the required size.
        /// </summary>
        public void GetDashes(float* dashes, uint dashesCount)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDashes);
            ((delegate*<void*, float*, uint, void>)functionPointer)(nativePointer, dashes, dashesCount);
        }
    }
}
