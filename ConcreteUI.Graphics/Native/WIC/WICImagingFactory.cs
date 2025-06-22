using System;
using System.IO;
using System.Runtime.CompilerServices;

using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;
using WitherTorch.Common.Windows.ObjectModels;
using WitherTorch.Common.Windows.ObjectModels.Adapters;

namespace ConcreteUI.Graphics.Native.WIC
{
    public sealed unsafe class WICImagingFactory : ComObject
    {
        public static readonly Guid IID_IWICImagingFactory = new Guid(0xec5ec8a9, 0xc395, 0x4314, 0x9c, 0x77, 0x54, 0xd7, 0xa9, 0x35, 0xff, 0x70);
        public static readonly Guid CLSID_WICImagingFactory;
        public static readonly Guid CLSID_WICImagingFactory1 = new Guid(0xcacaf262, 0x9370, 0x4615, 0xa1, 0x3b, 0x9f, 0x55, 0x39, 0xda, 0x4c, 0xa);
        public static readonly Guid CLSID_WICImagingFactory2 = new Guid(0x317d06e8, 0x5f24, 0x433d, 0xbd, 0xf7, 0x79, 0xce, 0x68, 0xd8, 0xab, 0xc2);

        private new enum MethodTable
        {
            _Start = ComObject.MethodTable._End,
            CreateDecoderFromFilename = _Start,
            CreateDecoderFromStream,
            CreateDecoderFromFileHandle,
            CreateComponentInfo,
            CreateDecoder,
            CreateEncoder,
            CreatePalette,
            CreateFormatConverter,
            CreateBitmapScaler,
            CreateBitmapClipper,
            CreateBitmapFlipRotator,
            CreateStream,
            CreateColorContext,
            CreateColorTransformer,
            CreateBitmap,
            CreateBitmapFromSource,
            CreateBitmapFromSourceRect,
            CreateBitmapFromMemory,
            CreateBitmapFromHBITMAP,
            CreateBitmapFromHICON,
            CreateComponentEnumerator,
            CreateFastMetadataEncoderFromDecoder,
            CreateFastMetadataEncoderFromFrameDecode,
            CreateQueryWriter,
            CreateQueryWriterFromReader,
            _End,
        }

        static WICImagingFactory()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2))
                CLSID_WICImagingFactory = CLSID_WICImagingFactory2;
            else
                CLSID_WICImagingFactory = CLSID_WICImagingFactory1;
        }

        public WICImagingFactory() : base() { }

        public WICImagingFactory(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

        public static WICImagingFactory? Create(bool throwWhenFailed = true)
            => CoCreateInstance<WICImagingFactory>(CLSID_WICImagingFactory, IID_IWICImagingFactory, throwWhenFailed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromFilename(string filename, FileAccess access, WICDecodeOptions metadataOptions)
            => CreateDecoderFromFilenameCore(filename, null, access, metadataOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromFilename(string filename, in Guid vendorGuid, FileAccess access, WICDecodeOptions metadataOptions)
            => CreateDecoderFromFilenameCore(filename, UnsafeHelper.AsPointerIn(in vendorGuid), access, metadataOptions);

        private WICBitmapDecoder CreateDecoderFromFilenameCore(string filename, Guid* vendorGuid, FileAccess access, WICDecodeOptions metadataOptions)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateDecoderFromFilename);
            int hr;
            fixed (char* ptr = filename)
            {
                hr = ((delegate*<void*, char*, Guid*, uint, WICDecodeOptions, void**, int>)functionPointer)(nativePointer,
                    ptr, vendorGuid, ConvertFileAccessToWin32GenericAccess(access), metadataOptions, &nativePointer);
            }
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapDecoder(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromStream(IWin32Stream stream, WICDecodeOptions metadataOptions)
            => CreateDecoderFromStream(stream, null, metadataOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromStream(IWin32Stream stream, in Guid vendorGuid, WICDecodeOptions metadataOptions)
            => CreateDecoderFromStream(stream, UnsafeHelper.AsPointerIn(in vendorGuid), metadataOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromStream(IWin32Stream stream, Guid* vendorGuid, WICDecodeOptions metadataOptions)
        {
            if (stream is IWin32HandleHolder holder)
                return CreateDecoderFromStreamCore(holder.GetWin32Handle(), vendorGuid, metadataOptions);
            using Win32StreamAdapter adapter = new Win32StreamAdapter(stream);
            return CreateDecoderFromStreamCore(adapter.GetWin32Handle(), vendorGuid, metadataOptions);
        }

        private WICBitmapDecoder CreateDecoderFromStreamCore(void* pStream, Guid* vendorGuid, WICDecodeOptions metadataOptions)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateDecoderFromStream);
            int hr = ((delegate*<void*, void*, Guid*, WICDecodeOptions, void**, int>)functionPointer)(nativePointer,
                    pStream, vendorGuid, metadataOptions, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapDecoder(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromFileHandle(IntPtr handle, WICDecodeOptions metadataOptions)
            => CreateDecoderFromFileHandleCore(handle, null, metadataOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WICBitmapDecoder CreateDecoderFromFileHandle(IntPtr handle, in Guid vendorGuid, WICDecodeOptions metadataOptions)
            => CreateDecoderFromFileHandleCore(handle, UnsafeHelper.AsPointerIn(in vendorGuid), metadataOptions);

        private WICBitmapDecoder CreateDecoderFromFileHandleCore(IntPtr handle, Guid* vendorGuid, WICDecodeOptions metadataOptions)
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateDecoderFromFileHandle);
            int hr = ((delegate*<void*, IntPtr, Guid*, WICDecodeOptions, void**, int>)functionPointer)(nativePointer,
                    handle, vendorGuid, metadataOptions, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapDecoder(nativePointer, ReferenceType.Owned);
        }

        public WICBitmapScaler CreateBitmapScaler()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapScaler);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapScaler(nativePointer, ReferenceType.Owned);
        }

        public WICBitmapClipper CreateBitmapClipper()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapClipper);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapClipper(nativePointer, ReferenceType.Owned);
        }

        public WICBitmapFlipRotator CreateBitmapFlipRotator()
        {
            void* nativePointer = NativePointer;
            void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateBitmapFlipRotator);
            int hr = ((delegate*<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
            ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
            return new WICBitmapFlipRotator(nativePointer, ReferenceType.Owned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ConvertFileAccessToWin32GenericAccess(FileAccess access)
        {
            uint result = 0;
            if ((access & FileAccess.Read) == FileAccess.Read)
                return result |= 0x80000000U;
            if ((access & FileAccess.Write) == FileAccess.Write)
                return result |= 0x40000000U;
            return result;
        }
    }
}
