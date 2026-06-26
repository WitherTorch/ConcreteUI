using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LocalsInit;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Graphics.Native.WIC;

public static unsafe class WIC
{
    private const string WINDOWS_CODEC_DLL = "WindowsCodecs.dll";

#if NET8_0_OR_GREATER
    [SuppressGCTransition]
#endif
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
