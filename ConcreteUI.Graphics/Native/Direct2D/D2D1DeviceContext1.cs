using System;
using System.Runtime.InteropServices;
using System.Security;

using ConcreteUI.Graphics.Native.Direct2D.Brushes;
using ConcreteUI.Graphics.Native.Direct2D.Geometry;

using InlineMethod;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    /// <summary>
    /// Enables creation and drawing of geometry realization objects.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public sealed unsafe class D2D1DeviceContext1 : D2D1DeviceContext
    {
        public static readonly Guid IID_DeviceContext1 = new Guid(0xd37f57e4, 0x6908, 0x459f, 0xa1, 0x99, 0xe7, 0x2f, 0x24, 0xf7, 0x99, 0x87);

        private new enum MethodTable
        {
            _Start = D2D1DeviceContext.MethodTable._End,
            CreateFilledGeometryRealization = _Start,
            CreateStrokedGeometryRealization,
            DrawGeometryRealization,
            _End
        }

        public D2D1DeviceContext1() : base() { }

        public D2D1DeviceContext1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public D2D1GeometryRealization CreateFilledGeometryRealization(D2D1Geometry geometry, float flatteningTolerance)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateFilledGeometryRealization);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, float, void**, int>)functionPointer)(nativePointer, geometry.NativePointer, flatteningTolerance, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1GeometryRealization(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public D2D1GeometryRealization CreateStrokedGeometryRealization(D2D1Geometry geometry, float flatteningTolerance, float strokeWidth)
            => CreateStrokedGeometryRealization(geometry, flatteningTolerance, strokeWidth, null);

        public D2D1GeometryRealization CreateStrokedGeometryRealization(D2D1Geometry geometry, float flatteningTolerance, float strokeWidth,
            D2D1StrokeStyle strokeStyle)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateStrokedGeometryRealization);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, float, float, void*, void**, int>)functionPointer)(nativePointer,
                geometry.NativePointer, flatteningTolerance, strokeWidth, strokeStyle == null ? null : strokeStyle.NativePointer, &nativePointer);
            if (hr >= 0)
                return nativePointer == null ? null : new D2D1GeometryRealization(nativePointer, ReferenceType.Owned);
            throw Marshal.GetExceptionForHR(hr);
        }

        public void DrawGeometryRealization(D2D1GeometryRealization geometryRealization, D2D1Brush brush)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.DrawGeometryRealization);
            ((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)functionPointer)(nativePointer, geometryRealization.NativePointer, brush.NativePointer);
        }
    }
}
