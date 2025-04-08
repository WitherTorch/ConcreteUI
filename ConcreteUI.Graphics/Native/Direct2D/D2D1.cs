using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

using ConcreteUI.Graphics.Helpers;

using InlineMethod;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Windows.Helpers;
using WitherTorch.Common;

namespace ConcreteUI.Graphics.Native.Direct2D
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class D2D1
    {
        private const string D2D1_DLL = "d2d1.dll";

        private static readonly void*[] _pointers = MethodImportHelper.GetImportedMethodPointers(D2D1_DLL,
            nameof(D2D1CreateDevice), nameof(D2D1ComputeMaximumScaleFactor));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int D2D1CreateDevice(void* dxgiDevice, D2D1CreationProperties* creationProperties, void** d2dDevice)
        {
            void* pointer = _pointers[0];
            if (pointer == null)
                return Constants.E_NOTIMPL;
            return ((delegate* unmanaged[Stdcall]<void*, D2D1CreationProperties*, void**, int>)pointer)(dxgiDevice, creationProperties, d2dDevice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float D2D1ComputeMaximumScaleFactor(in Matrix3x2 matrix)
            => D2D1ComputeMaximumScaleFactor(UnsafeHelper.AsPointerIn(in matrix));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float D2D1ComputeMaximumScaleFactor(Matrix3x2* matrix)
        {
            void* pointer = _pointers[1];
            if (pointer == null)
                return 1.0f;
            return ((delegate* unmanaged[Stdcall]<Matrix3x2*, float>)pointer)(matrix);
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static float ComputeFlatteningTolerance()
            => Constants.D2D1_DEFAULT_FLATTENING_TOLERANCE / D2D1ComputeMaximumScaleFactor(Matrix3x2.Identity);

        [Inline(InlineBehavior.Keep, export: true)]
        public static float ComputeFlatteningTolerance(in Matrix3x2 matrix)
            => Constants.D2D1_DEFAULT_FLATTENING_TOLERANCE / D2D1ComputeMaximumScaleFactor(matrix);

        [Inline(InlineBehavior.Keep, export: true)]
        public static float ComputeFlatteningTolerance(in Matrix3x2 matrix, float dpiX, float dpiY, float maxZoomFactor)
        {
            Matrix3x2 dpiDependentTransform = matrix * Matrix3x2.CreateScale(dpiX / 96.0f, dpiY / 96.0f);

            float absMaxZoomFactor = maxZoomFactor > 0 ? maxZoomFactor : -maxZoomFactor;

            return Constants.D2D1_DEFAULT_FLATTENING_TOLERANCE /
                (absMaxZoomFactor * D2D1ComputeMaximumScaleFactor(&dpiDependentTransform));
        }
    }
}
