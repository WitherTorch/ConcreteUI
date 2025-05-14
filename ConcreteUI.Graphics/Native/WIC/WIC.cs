using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Native.WIC
{
    public static unsafe class WIC
    {
        private const string WINDOWS_CODEC_DLL = "WindowsCodecs.dll";

        [DllImport(WINDOWS_CODEC_DLL)]
        public static extern int WICConvertBitmapSource(Guid* dstFormat, void* pISrc, void** ppIDst);

        [LocalsInit(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WICBitmapSource? WICConvertBitmapSource(WICBitmapSource src, in Guid dstFormat)
        {
            void* result;
            int hr = WICConvertBitmapSource(UnsafeHelper.AsPointerIn(in dstFormat), src.NativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result == null ? null : new WICBitmapSource(result, ReferenceType.Owned);
        }
    }
}
