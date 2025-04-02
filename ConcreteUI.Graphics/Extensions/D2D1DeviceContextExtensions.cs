using System;
using System.Drawing;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.CrossNative;
using WitherTorch.CrossNative.Windows.Structures;

namespace ConcreteUI.Graphics.Extensions
{
    public static class D2D1DeviceContextExtensions
    {
        public unsafe static D2D1Bitmap LoadBitmap(this D2D1DeviceContext deviceContext, Bitmap bitmap)
        {
            var sourceArea = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapProperties = new D2D1BitmapProperties(new D2D1PixelFormat(DXGIFormat.R8G8B8A8_UNorm, D2D1AlphaMode.Premultiplied));
            var size = new SizeU(bitmap.Width.MakeUnsigned(), bitmap.Height.MakeUnsigned());
            int stride = bitmap.Width * sizeof(int);
            int dataCount = bitmap.Height * stride;
            if (dataCount < Limits.MaxStackallocBytes / sizeof(int))
            {
                uint* tempData = stackalloc uint[dataCount];
                uint* movablePtr = tempData;
                var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                int bmpDataStride = bitmapData.Stride;
                int offset = 0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++, movablePtr++)
                    {
                        IntPtr data = bitmapData.Scan0;
                        byte B = Marshal.ReadByte(data, offset++);
                        byte G = Marshal.ReadByte(data, offset++);
                        byte R = Marshal.ReadByte(data, offset++);
                        byte A = Marshal.ReadByte(data, offset++);
                        unchecked
                        {
                            *movablePtr = (uint)(R | G << 8 | B << 16 | A << 24);
                        }
                    }
                }
                bitmap.UnlockBits(bitmapData);
                if (deviceContext.IsDisposed)
                    return null;
                return deviceContext.CreateBitmap(size, tempData, stride.MakeUnsigned(), bitmapProperties);
            }
            else
            {
                fixed (uint* tempData = new uint[bitmap.Height * stride])
                {
                    uint* movablePtr = tempData;
                    var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    int bmpDataStride = bitmapData.Stride;
                    int offset = 0;
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++, movablePtr++)
                        {
                            IntPtr data = bitmapData.Scan0;
                            byte B = Marshal.ReadByte(data, offset++);
                            byte G = Marshal.ReadByte(data, offset++);
                            byte R = Marshal.ReadByte(data, offset++);
                            byte A = Marshal.ReadByte(data, offset++);
                            unchecked
                            {
                                *movablePtr = (uint)(R | G << 8 | B << 16 | A << 24);
                            }
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                    if (deviceContext.IsDisposed)
                        return null;
                    return deviceContext.CreateBitmap(size, tempData, stride.MakeUnsigned(), bitmapProperties);
                }
            }
        }

        public static D2D1Bitmap TakeScreenShot(this D2D1DeviceContext deviceContext)
        {
            D2D1Bitmap result = deviceContext.CreateBitmap(deviceContext.PixelSize, new D2D1BitmapProperties(deviceContext.PixelFormat));
            result.CopyFromRenderTarget(deviceContext);
            return result;
        }

        [Inline(InlineBehavior.Keep, export: true)]
        public static D2D1Bitmap TakeScreenShot(this D2D1DeviceContext deviceContext, in Rect clipRect) => TakeScreenShot(deviceContext, (RectU)clipRect);

        public static D2D1Bitmap TakeScreenShot(this D2D1DeviceContext deviceContext, in RectU clipRect)
        {
            D2D1Bitmap result = deviceContext.CreateBitmap(deviceContext.PixelSize, new D2D1BitmapProperties(deviceContext.PixelFormat));
            result.CopyFromRenderTarget(new PointU(0, 0), deviceContext, clipRect);
            return result;
        }
    }
}
