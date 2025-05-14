using System;

using InlineMethod;

using LocalsInit;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.Extensions;
using WitherTorch.Common.Windows.ObjectModels;
using WitherTorch.Common.Windows.ObjectModels.Adapters;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICBitmapDecoder : ComObject
    {
        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            QueryCapability = _Start,
            Initialize,
            GetContainerFormat,
            GetDecoderInfo,
            CopyPalette,
            GetMetadataQueryReader,
            GetPreview,
            GetColorContexts,
            GetThumbnail,
            GetFrameCount,
            GetFrame,
            _End,
        }

        public WICBitmapDecoder() : base() { }

        public WICBitmapDecoder(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public uint FrameCount => GetFrameCount();

        public WICBitmapFrameDecode? this[uint index]
        {
            get => GetFrame(index);
        }

        public WICBitmapDecoderCapabilities QueryCapacity(IWin32Stream stream)
        {
            if (stream is IWin32HandleHolder holder)
                return QueryCapacityCore(holder.GetWin32Handle());
            using Win32StreamAdapter adapter = new Win32StreamAdapter(stream);
            return QueryCapacityCore(adapter.GetWin32Handle());
        }

        [LocalsInit(false)]
        private WICBitmapDecoderCapabilities QueryCapacityCore(void* pStream)
        {
            WICBitmapDecoderCapabilities result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.QueryCapability);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, WICBitmapDecoderCapabilities*, int>)functionPointer)(nativePointer, pStream, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        public void Initialize(IWin32Stream stream, WICDecodeOptions cacheOptions)
        {
            if (stream is IWin32HandleHolder holder)
            {
                InitializeCore(holder.GetWin32Handle(), cacheOptions);
                return;
            }
            using Win32StreamAdapter adapter = new Win32StreamAdapter(stream);
            InitializeCore(adapter.GetWin32Handle(), cacheOptions);
        }

        private void InitializeCore(void* pStream, WICDecodeOptions cacheOptions)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Initialize);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void*, WICDecodeOptions, int>)functionPointer)(nativePointer, pStream, cacheOptions);
            ThrowHelper.ThrowExceptionForHR(hr);
        }

        [LocalsInit(false)]
        public Guid GetContainerFormat()
        {
            Guid result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetContainerFormat);
            int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        public WICBitmapSource? GetPreview()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPreview);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new WICBitmapSource(nativePointer, ReferenceType.Owned);
        }

        public WICBitmapSource? GetThumbnail()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetThumbnail);
            int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new WICBitmapSource(nativePointer, ReferenceType.Owned);
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private uint GetFrameCount()
        {
            uint result;
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFrameCount);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint*, int>)functionPointer)(nativePointer, &result);
            ThrowHelper.ThrowExceptionForHR(hr);
            return result;
        }

        [LocalsInit(false)]
        [Inline(InlineBehavior.Remove)]
        private WICBitmapFrameDecode? GetFrame(uint index)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetFrame);
            int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, index, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr);
            return nativePointer == null ? null : new WICBitmapFrameDecode(nativePointer, ReferenceType.Owned);
        }
    }
}
