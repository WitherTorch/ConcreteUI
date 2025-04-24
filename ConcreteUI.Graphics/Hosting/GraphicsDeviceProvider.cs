using System;
using System.Runtime.CompilerServices;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.Direct3D;
using ConcreteUI.Graphics.Native.Direct3D11;
using ConcreteUI.Graphics.Native.DXGI;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics.Hosting
{
    public unsafe sealed class GraphicsDeviceProvider : IDisposable
    {
        private const D3D11CreateDeviceFlags CreateDeviceFlags = D3D11CreateDeviceFlags.BgraSupport;
        private const D3D11CreateDeviceFlags CreateDeviceFlagsForDebug = CreateDeviceFlags | D3D11CreateDeviceFlags.Debug;

        public readonly DXGIAdapter DXGIAdapter;
        public readonly DXGIFactory DXGIFactory;
        public readonly DXGIDevice DXGIDevice;
        public readonly D3D11Device D3DDevice;
        public readonly D2D1Device D2DDevice;

        private bool _disposed;

        private GraphicsDeviceProvider(D3D11Device? d3dDevice, DXGIAdapter? adapter, DXGIFactory? factory, bool isDebug)
        {
            // 當硬體 3D 裝置建立失敗時，改建立 WARP 3D 裝置
            d3dDevice ??= NullSafetyHelper.ThrowIfNull(D3D11Device.Create(null, D3DDriverType.Warp, IntPtr.Zero,
                isDebug ? CreateDeviceFlagsForDebug : CreateDeviceFlags));

            D3DDevice = d3dDevice;
            DXGIDevice = NullSafetyHelper.ThrowIfNull(d3dDevice.QueryInterface<DXGIDevice>(DXGIDevice.IID_DXGIDevice));

            D2DDevice = D2D1Device.Create(DXGIDevice, new D2D1CreationProperties()
            {
                Options = D2D1DeviceContextOptions.None,
                DebugLevel = isDebug ? D2D1DebugLevel.Information : D2D1DebugLevel.None,
                ThreadingMode = D2D1ThreadingMode.MultiThreaded
            });

            adapter ??= DXGIDevice.GetAdapter();

            DXGIAdapter = adapter;

            if (factory is null)
            {
                factory = adapter.GetParent<DXGIFactory6>(DXGIFactory6.IID_DXGIFactory6, throwException: false);
                factory ??= adapter.GetParent<DXGIFactory2>(DXGIFactory2.IID_DXGIFactory2, throwException: false);
                factory ??= adapter.GetParent<DXGIFactory1>(DXGIFactory1.IID_DXGIFactory1, throwException: false);
                factory ??= adapter.GetParent<DXGIFactory>(DXGIFactory.IID_DXGIFactory, throwException: true);
                DXGIFactory = factory!;
                return;
            }

            DXGIFactory = GetLatestDXGIFactoryInterface(factory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIFactory GetLatestDXGIFactoryInterface(DXGIFactory factory)
        {
            if (factory is DXGIFactory1)
                return factory;

            DXGIFactory? result;

            if ((result = factory.QueryInterface<DXGIFactory6>(DXGIFactory6.IID_DXGIFactory6, throwWhenQueryFailed: false)) is not null)
            {
                factory.Dispose();
                return result;
            }

            if (factory is DXGIFactory2)
                return factory;

            if ((result = factory.QueryInterface<DXGIFactory2>(DXGIFactory2.IID_DXGIFactory2, throwWhenQueryFailed: false)) is not null)
            {
                factory.Dispose();
                return result;
            }

            if (factory is DXGIFactory1)
                return factory;

            if ((result = factory.QueryInterface<DXGIFactory1>(DXGIFactory1.IID_DXGIFactory1, throwWhenQueryFailed: false)) is not null)
            {
                factory.Dispose();
                return result;
            }

            return factory;
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
            DXGIFactory? result = DXGIFactory2.Create(DXGICreateFactoryFlags.None, DXGIFactory2.IID_DXGIFactory2, throwException: false);
            if (result is not null)
                return result;
            result = DXGIFactory1.Create(DXGIFactory1.IID_DXGIFactory1, throwException: false);
            if (result is not null)
                return result;
            return NullSafetyHelper.ThrowIfNull(DXGIFactory.Create(DXGIFactory.IID_DXGIFactory, throwException: true));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DXGIAdapter? SearchBestAdapter(ref DXGIFactory factory, DXGIGpuPreference preference)
        {
            DXGIAdapter? result;
            if (factory is not DXGIFactory6 factory6)
            {
                factory6 = factory.QueryInterface<DXGIFactory6>(DXGIFactory6.IID_DXGIFactory6, throwWhenQueryFailed: false)!;
                if (factory6 is null)
                    return factory.EnumAdapters(0, throwException: false);
                DisposeHelper.SwapDispose(ref factory!, factory6);
            }
            result = factory6.EnumAdapterByGpuPreference(0, preference, DXGIAdapter.IID_DXGIAdapter, throwException: false);
            if (result is not null)
                return result;
            return factory.EnumAdapters(0, throwException: false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (!disposing)
                return;
            DXGIFactory?.Dispose();
            DXGIAdapter?.Dispose();
            D3DDevice?.Dispose();
            DXGIDevice?.Dispose();
            D2DDevice?.Dispose();
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
