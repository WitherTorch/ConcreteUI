using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct3D;
using ConcreteUI.Graphics.Native.Direct3D11;
using ConcreteUI.Graphics.Native.DirectComposition;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common;
using WitherTorch.Common.Helpers;
using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics
{
    public unsafe sealed class GraphicsDeviceProvider : ICheckableDisposable
    {
        private const bool UseLegacyRoute = false;

        private const D3D11CreateDeviceFlags CreateDeviceFlags = D3D11CreateDeviceFlags.BgraSupport;
        private const D3D11CreateDeviceFlags CreateDeviceFlagsForDebug = CreateDeviceFlags | D3D11CreateDeviceFlags.Debug;

        private readonly DXGIAdapter _dxgiAdapter;
        private readonly DXGIFactory _dxgiFactory;
        private readonly D3D11Device _d3dDevice;
        private readonly DXGIDevice _dxgiDevice;
        private readonly D2D1Factory _d2dFactory;
        private readonly D2D1Device? _d2dDevice;
        private readonly DCompositionDevice? _dcompDevice;
        private readonly bool _supportSwapChain1, _supportDComp;

        private bool _disposed;

        public bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _disposed;
        }

        public DXGIAdapter DXGIAdapter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dxgiAdapter;
        }

        public DXGIFactory DXGIFactory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dxgiFactory;
        }

        public D3D11Device D3DDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _d3dDevice;
        }

        public DXGIDevice DXGIDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dxgiDevice;
        }

        public D2D1Factory D2DFactory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _d2dFactory;
        }

        public D2D1Device? D2DDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _d2dDevice;
        }

        public DCompositionDevice? DCompDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dcompDevice;
        }

        public bool IsSupportSwapChain1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _supportSwapChain1;
        }

        public bool IsSupportDComp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _supportDComp;
        }

        private GraphicsDeviceProvider(D3D11Device? d3dDevice, DXGIAdapter? adapter, DXGIFactory? factory, bool isDebug)
        {
            // 當硬體 3D 裝置建立失敗時，改建立 WARP 3D 裝置
            d3dDevice ??= NullSafetyHelper.ThrowIfNull(D3D11Device.Create(null, D3DDriverType.Warp, IntPtr.Zero,
                isDebug ? CreateDeviceFlagsForDebug : CreateDeviceFlags));

            _d3dDevice = d3dDevice;
            DXGIDevice dxgiDevice = GetLatestDXGIDeviceInterface(NullSafetyHelper.ThrowIfNull(d3dDevice.QueryInterface<DXGIDevice>(DXGIDevice.IID_IDXGIDevice)));

            if (dxgiDevice is DXGIDevice1 dxgiDevice1)
                dxgiDevice1.MaximumFrameLatency = 1;

            _dxgiDevice = dxgiDevice;

            adapter ??= dxgiDevice.GetAdapter();

            _dxgiAdapter = adapter;

            if (factory is null)
            {
                factory = adapter.GetParent<DXGIFactory6>(DXGIFactory6.IID_IDXGIFactory6, throwException: false);
                factory ??= adapter.GetParent<DXGIFactory2>(DXGIFactory2.IID_IDXGIFactory2, throwException: false);
                factory ??= adapter.GetParent<DXGIFactory1>(DXGIFactory1.IID_IDXGIFactory1, throwException: false);
                factory ??= NullSafetyHelper.ThrowIfNull(adapter.GetParent<DXGIFactory>(DXGIFactory.IID_IDXGIFactory, throwException: true));
            }
            else
            {
                factory = GetLatestDXGIFactoryInterface(factory);
            }
            _dxgiFactory = factory;

            if (UseLegacyRoute || factory is not DXGIFactory2 || !TryCreateD2DDevice(dxgiDevice, isDebug, out D2D1Device? d2dDevice))
            {
                _supportSwapChain1 = false;
                _supportDComp = false;
                _d2dFactory = D2D1Factory.Create(D2D1FactoryType.MultiThreaded, new D2D1FactoryOptions()
                {
                    DebugLevel = isDebug ? D2D1DebugLevel.Information : D2D1DebugLevel.None,
                });
            }
            else
            {
                _supportSwapChain1 = true;
                _d2dDevice = d2dDevice;
                _d2dFactory = d2dDevice.GetFactory();
                _supportDComp = TryCreateDCompDevice(dxgiDevice, out _dcompDevice);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIDevice GetLatestDXGIDeviceInterface(DXGIDevice device)
        {
            if (device is DXGIDevice1)
                goto NotFound;

            DXGIDevice? result;

            if ((result = device.QueryInterface<DXGIDevice1>(DXGIDevice1.IID_IDXGIDevice1, throwWhenQueryFailed: false)) is not null)
                goto Found;

        NotFound:
            return device;

        Found:
            device.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIFactory GetLatestDXGIFactoryInterface(DXGIFactory factory)
        {
            if (factory is DXGIFactory6)
                goto NotFound;

            DXGIFactory? result;

            if ((result = factory.QueryInterface<DXGIFactory6>(DXGIFactory6.IID_IDXGIFactory6, throwWhenQueryFailed: false)) is not null)
                goto Found;

            if (factory is DXGIFactory2)
                goto NotFound;

            if ((result = factory.QueryInterface<DXGIFactory2>(DXGIFactory2.IID_IDXGIFactory2, throwWhenQueryFailed: false)) is not null)
                goto Found;

            if (factory is DXGIFactory1)
                goto NotFound;

            if ((result = factory.QueryInterface<DXGIFactory1>(DXGIFactory1.IID_IDXGIFactory1, throwWhenQueryFailed: false)) is not null)
                goto Found;

            goto NotFound;

        NotFound:
            return factory;

        Found:
            factory.Dispose();
            return result;
        }

        public GraphicsDeviceProvider(DXGIGpuPreference preference, bool isDebug) :
            this(CreateDevice(preference, null, isDebug, out DXGIAdapter? adapter, out DXGIFactory? factory), adapter, factory, isDebug)
        { }

        public GraphicsDeviceProvider(string targetGpuName, bool isDebug) :
            this(CreateDevice(DXGIGpuPreference.Unspecified, targetGpuName, isDebug, out DXGIAdapter? adapter, out DXGIFactory? factory), adapter, factory, isDebug)
        { }

        private static D3D11Device? CreateDevice(DXGIGpuPreference preference, string? targetGpuName, bool isDebug, out DXGIAdapter? adapter, out DXGIFactory? factory)
        {
            if (preference >= DXGIGpuPreference.Invalid)
            {
                adapter = null;
                factory = null;
                return null;
            }

            factory = CreateDXGIFactory();

            if (factory is null)
            {
                adapter = null;
                factory = null;
                return null;
            }

            if (StringHelper.IsNullOrEmpty(targetGpuName))
            {
                adapter = SearchBestAdapter(ref factory, preference);
            }
            else
            {
                adapter = null;
                for (uint i = 0; i < Constants.AdapterEnumerationLimit; i++)
                {
                    DXGIAdapter? _adapter = factory.EnumAdapters(i, throwException: false);

                    if (_adapter is null)
                        break;

                    DXGIAdapterDescription description = _adapter.Description;
                    if (description.VendorId == 5140) //is "Microsoft Basic Render Driver"     
                    {
                        _adapter.Dispose();
                        continue;
                    }
                    if (string.Equals(description.Description.ToString(), targetGpuName))
                    {
                        adapter = _adapter;
                        break;
                    }
                }
            }

            if (adapter is null)
                return null;
            return D3D11Device.Create(adapter, D3DDriverType.Unknown, IntPtr.Zero, isDebug ? CreateDeviceFlagsForDebug : CreateDeviceFlags, Constants.FeatureLevels);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIFactory CreateDXGIFactory()
        {
            DXGIFactory? result = DXGIFactory2.Create(DXGICreateFactoryFlags.None, DXGIFactory2.IID_IDXGIFactory2, throwException: false);
            if (result is not null)
                return result;
            result = DXGIFactory1.Create(DXGIFactory1.IID_IDXGIFactory1, throwException: false);
            if (result is not null)
                return result;
            return NullSafetyHelper.ThrowIfNull(DXGIFactory.Create(DXGIFactory.IID_IDXGIFactory, throwException: true));
        }

        private static bool TryCreateD2DDevice(DXGIDevice device, bool isDebug, 
            [NotNullWhen(true)] out D2D1Device? result)
            => D2D1Device.TryCreate(device, new D2D1CreationProperties()
            {
                Options = D2D1DeviceContextOptions.None,
                DebugLevel = isDebug ? D2D1DebugLevel.Information : D2D1DebugLevel.None,
                ThreadingMode = D2D1ThreadingMode.MultiThreaded
            }, out result);

        private static bool TryCreateDCompDevice(DXGIDevice device, [NotNullWhen(true)] out DCompositionDevice? result)
        {
            Guid iid = DCompositionDevice.IID_IDCompositionDevice;
            void* nativePointer = device.NativePointer;
            int hr = DComp.DCompositionCreateDevice(nativePointer, &iid, &nativePointer);
            if (hr < 0)
            {
                result = null;
                return false;
            }
            result = NativeObject.FromNativePointer<DCompositionDevice>(nativePointer, ReferenceType.Owned);
            return result is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIAdapter? SearchBestAdapter(ref DXGIFactory factory, DXGIGpuPreference preference)
        {
            DXGIAdapter? result;
            if (factory is not DXGIFactory6 factory6)
            {
                factory6 = factory.QueryInterface<DXGIFactory6>(DXGIFactory6.IID_IDXGIFactory6, throwWhenQueryFailed: false)!;
                if (factory6 is null)
                    return factory.EnumAdapters(0, throwException: false);
                DisposeHelper.SwapDispose(ref factory!, factory6);
            }
            result = factory6.EnumAdapterByGpuPreference(0, preference, DXGIAdapter.IID_IDXGIAdapter, throwException: false);
            if (result is not null)
                return result;
            return factory.EnumAdapters(0, throwException: false);
        }

        private void Dispose(bool disposing)
        {
            if (ReferenceHelper.Exchange(ref _disposed, true) || !disposing)
                return;
            _dxgiAdapter.Dispose();
            _dxgiFactory.Dispose();
            _d3dDevice.Dispose();
            _dxgiDevice.Dispose();
            _d2dFactory.Dispose();
            _d2dDevice?.Dispose();
            _dcompDevice?.Dispose();
        }

        ~GraphicsDeviceProvider()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
