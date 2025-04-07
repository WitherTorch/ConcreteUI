using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics.Extensions;
using ConcreteUI.Graphics.Helpers;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Graphics.Hosting
{
    public class SwapChainGraphicsHost : GraphicsHostBase
    {
        protected readonly DXGISwapChain _swapChain;

        private readonly GraphicsDeviceProvider _provider;
        private readonly D2D1TextAntialiasMode _antialiasMode;
        private readonly D2D1DeviceContext _context;

        protected bool _disposed;
        protected bool _sleeping;

        private D2D1DeviceContext _activeContext;
        private D2D1Bitmap1 _target;

        private bool _alternateFlushing;

        public override bool IsDisposed => _disposed;

        public SwapChainGraphicsHost(GraphicsDeviceProvider provider, IntPtr handle, D2D1TextAntialiasMode textAntialiasMode, bool isFlipModel) : base(handle)
        {
            D2D1DeviceContext context = TryQueryNewestInterface(provider.D2DDevice.CreateDeviceContext(D2D1DeviceContextOptions.None));
            DXGISwapChain swapChain = CreateSwapChain(provider, isFlipModel);
            DisableDXGIExtendedFeature(swapChain);
            context.AntialiasMode = D2D1AntialiasMode.Aliased;
            context.TextAntialiasMode = textAntialiasMode;
            _provider = provider;
            _context = context;
            _antialiasMode = textAntialiasMode;
            _swapChain = swapChain;
            _alternateFlushing = false;
        }

        public SwapChainGraphicsHost(SwapChainGraphicsHost another, IntPtr handle) : base(handle)
        {
            GraphicsDeviceProvider provider = another._provider;
            D2D1DeviceContext context = TryQueryNewestInterface(provider.D2DDevice.CreateDeviceContext(D2D1DeviceContextOptions.None));
            DXGISwapChain swapChain = CreateSwapChain(provider, another._swapChain);
            DisableDXGIExtendedFeature(swapChain);
            context.AntialiasMode = another._context.AntialiasMode;
            context.TextAntialiasMode = another._antialiasMode;
            _provider = provider;
            _context = context;
            _antialiasMode = another._antialiasMode;
            _swapChain = swapChain;
            _alternateFlushing = another._alternateFlushing;
        }

        protected virtual DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, bool isFlipModel)
        {
            DXGISwapChainDescription swapChainDesc = new DXGISwapChainDescription
            {
                BufferUsage = DXGIUsage.RenderTargetOutput | DXGIUsage.BackBuffer,
                OutputWindow = Handle,
                Windowed = true,
                BufferDesc = new DXGIModeDescription(Constants.Format),
                SampleDesc = new DXGISampleDescription(1, 0)
            };
            if (isFlipModel)
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SwapEffect = DXGISwapEffect.FlipDiscard;
            }
            else
            {
                swapChainDesc.BufferCount = 1;
                swapChainDesc.SwapEffect = DXGISwapEffect.Discard;
            }
            return provider.DXGIFactory.CreateSwapChain(provider.D3DDevice, swapChainDesc);
        }

        protected virtual DXGISwapChain CreateSwapChain(GraphicsDeviceProvider provider, DXGISwapChain original)
        {
            DXGISwapChainDescription swapChainDesc = original.Description;
            swapChainDesc.OutputWindow = Handle;
            return provider.DXGIFactory.CreateSwapChain(provider.D3DDevice, swapChainDesc);
        }

        [Inline(InlineBehavior.Remove)]
        private void DisableDXGIExtendedFeature(DXGISwapChain swapChain)
        {
            DXGIFactory factory = swapChain.GetParent<DXGIFactory>(DXGIFactory.IID_DXGIFactory);
            factory.MakeWindowAssociation(Handle,
                DXGIMakeWindowAssociationFlags.NoAltEnter | DXGIMakeWindowAssociationFlags.NoWindowChanges | DXGIMakeWindowAssociationFlags.NoPrintScreen);
            factory.Dispose();
        }

        [Inline(InlineBehavior.Remove)]
        private static D2D1DeviceContext TryQueryNewestInterface(D2D1DeviceContext context)
        {
            D2D1DeviceContext1 context1 = context.QueryInterface<D2D1DeviceContext1>(D2D1DeviceContext1.IID_DeviceContext1, throwWhenQueryFailed: false);
            if (context1 is null)
                return context;
            context.Dispose();
            return context1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GraphicsDeviceProvider GetDeviceProvider() => _provider;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1DeviceContext GetDeviceContext() => _context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1DeviceContext BeginDraw()
        {
            D2D1DeviceContext context = _activeContext;
            if (context is not null)
                return context;
            context = _context;
            if (context.IsDisposed)
                return null;
            BeginDrawCore(context);
            _activeContext = context;
            return context;
        }

        [Inline(InlineBehavior.Remove)]
        private void BeginDrawCore(D2D1DeviceContext context)
        {
            D2D1Bitmap1 target = _target;
            if (target is null)
            {
                DXGISurface surface = _swapChain.GetBuffer<DXGISurface>(0, DXGISurface.IID_DXGISurface);
                target = context.CreateBitmapFromDxgiSurface(surface);
                surface.Dispose();
                _target = target;
            }
            context.Target = target;
            context.BeginDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndDraw()
        {
            D2D1DeviceContext context = _activeContext;
            if (context is null)
                return;
            _activeContext = null;
            if (context.IsDisposed)
                return;
            EndDrawCore(context);
        }

        [Inline(InlineBehavior.Remove)]
        private void EndDrawCore(D2D1DeviceContext context)
        {
#if DEBUG
            context.EndDraw();
#else
            context.TryEndDraw();
#endif
            context.Target = null;
            DisposeHelper.SwapDispose(ref _target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPresent()
        {
            return PresentCore() is null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Present()
        {
            Exception exception = PresentCore();
            if (exception is null)
                return;
            throw exception;
        }

        private Exception PresentCore()
        {
            DXGISwapChain swapChain = _swapChain;
            if (BeforeNormalPresent(swapChain))
                return null;
            Exception exception = GetExceptionHRForPresent(swapChain, PresentCore(swapChain));
            return exception;
        }

        [Inline(InlineBehavior.Remove)]
        protected static int PresentCore(DXGISwapChain swapChain) => swapChain.TryPresent(0, DXGIPresentFlags.None);

        [Inline(InlineBehavior.Remove)]
        protected bool BeforeNormalPresent(DXGISwapChain swapChain)
        {
            if (_sleeping)
            {
                if (swapChain.TryPresent(0, DXGIPresentFlags.Test) != Constants.DXGI_STATUS_OCCLUDED)
                    _sleeping = false;
                return true;
            }
            return false;
        }

        protected virtual Exception GetExceptionHRForPresent(DXGISwapChain swapChain, int hr)
        {
            if (hr >= 0)
            {
                if (hr == Constants.DXGI_STATUS_OCCLUDED)
                    _sleeping = true;
                return null;
            }
            return hr switch
            {
                Constants.DXGI_ERROR_DEVICE_REMOVED => new InvalidOperationException("Direct3D Device or DXGI Device has been removed."),
                Constants.E_OUTOFMEMORY => new InvalidOperationException("Direct3D Device or DXGI Device was out of memory, application must be shutdown!"),
                _ => Marshal.GetExceptionForHR(hr),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Flush()
        {
            D2D1DeviceContext context = _activeContext;
            if (context is null || context.IsDisposed)
                return;
            if (_alternateFlushing)
            {
                AlternativeFlush(context);
                return;
            }
            int hr = context.TryFlush();
            if (hr >= 0)
                return;
            if (hr == Constants.E_NOTIMPL)
            {
                _alternateFlushing = true;
                AlternativeFlush(context);
                return;
            }
#if DEBUG
            throw Marshal.GetExceptionForHR(hr);
#endif
        }

        [Inline(InlineBehavior.Remove)]
        private static void AlternativeFlush(D2D1DeviceContext context)
        {
            context.TryEndDraw();
            context.BeginDraw();
        }

        public void Resize(in Size size)
        {
            DXGISwapChain swapChain = _swapChain;
            if (swapChain.IsDisposed)
                return;
            bool beginDrawCalled;
            D2D1DeviceContext context = _activeContext;
            if (context is null)
            {
                beginDrawCalled = false;
                context = _context;
            }
            else
            {
                beginDrawCalled = true;
                EndDrawCore(context);
            }
            if (context.IsDisposed)
                return;
            ResizeSwapChain(swapChain, size);
            if (!beginDrawCalled)
                return;
            BeginDrawCore(context);
            _activeContext = context;
        }

        protected virtual void ResizeSwapChain(DXGISwapChain swapChain, in Size size)
            => swapChain.ResizeBuffers(0, MathHelper.MakeUnsigned(size.Width), MathHelper.MakeUnsigned(size.Height));

        #region Disposing
        private void DisposeCore(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            if (disposing)
                _activeContext = null;
            _context.Dispose();
            _target?.Dispose();
            _swapChain.Dispose();
        }

        ~SwapChainGraphicsHost()
        {
            DisposeCore(disposing: false);
        }

        public override void Dispose()
        {
            DisposeCore(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
