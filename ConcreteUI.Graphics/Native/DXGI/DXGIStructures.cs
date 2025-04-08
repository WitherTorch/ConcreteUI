using System;
using System.Drawing;
using System.Runtime.InteropServices;

using WitherTorch.Common.Windows.Structures;

namespace ConcreteUI.Graphics.Native.DXGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DXGISwapChainDescription
    {
        public DXGIModeDescription BufferDesc;
        public DXGISampleDescription SampleDesc;
        public DXGIUsage BufferUsage;
        public uint BufferCount;
        public IntPtr OutputWindow;
        public SysBool Windowed;
        public DXGISwapEffect SwapEffect;
        public DXGISwapChainFlags Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGISwapChainDescription1
    {
        public uint Width;
        public uint Height;
        public DXGIFormat Format;
        public SysBool Stereo;
        public DXGISampleDescription SampleDesc;
        public DXGIUsage BufferUsage;
        public uint BufferCount;
        public DXGIScaling Scaling;
        public DXGISwapEffect SwapEffect;
        public DXGIAlphaMode AlphaMode;
        public DXGISwapChainFlags Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGISwapChainFullscreenDescription
    {
        public DXGIRational RefreshRate;
        public DXGIModeScanlineOrder ScanlineOrdering;
        public DXGIModeScaling Scaling;
        public SysBool Windowed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DXGIPresentParameters
    {
        public uint DirtyRectsCount;
        public Rect* pDirtyRects;
        public Rect* pScrollRect;
        public Point* pScrollOffset;

        public DXGIPresentParameters(uint dirtyRectsCount, Rect* pDirtyRects)
        {
            DirtyRectsCount = dirtyRectsCount;
            this.pDirtyRects = pDirtyRects;
            pScrollRect = null;
            pScrollOffset = null;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGIModeDescription
    {
        public uint Width;
        public uint Height;
        public DXGIRational RefreshRate;
        public DXGIFormat Format;
        public DXGIModeScanlineOrder ScanlineOrdering;
        public DXGIModeScaling Scaling;

        public DXGIModeDescription(DXGIFormat format)
        {
            this = default;
            Format = format;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGISampleDescription
    {
        public uint Count;
        public uint Quality;

        public DXGISampleDescription(uint count, uint quality)
        {
            Count = count;
            Quality = quality;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGIRational
    {
        public uint Numerator;
        public uint Denominator;

        public DXGIRational(uint numerator, uint denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public unsafe struct DXGIAdapterDescription
    {
        public FixedChar128 Description;
        public uint VendorId;
        public uint DeviceId;
        public uint SubSysId;
        public uint Revision;
        public IntPtr DedicatedVideoMemory;
        public IntPtr DedicatedSystemMemory;
        public IntPtr SharedSystemMemory;
        public Luid AdapterLuid;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public unsafe struct DXGIAdapterDescription1
    {
        public FixedChar128 Description;
        public uint VendorId;
        public uint DeviceId;
        public uint SubSysId;
        public uint Revision;
        public IntPtr DedicatedVideoMemory;
        public IntPtr DedicatedSystemMemory;
        public IntPtr SharedSystemMemory;
        public Luid AdapterLuid;
        public DXGIAdapterFlags Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXGISurfaceDescription
    {
        public uint Width;
        public uint Height;
        public DXGIFormat Format;
        public DXGISampleDescription SampleDesc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DXGIMappedRect
    {
        public int Pitch;
        public byte* pBits;
    }
}
