using ConcreteUI.Graphics.Hosting;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

using InlineMethod;

using System;
using System.Collections.Generic;

namespace ConcreteUI.Graphics.Helpers
{
    public static class GraphicsHostHelper
    {
        [Inline(InlineBehavior.Keep, export: true)]
        public static SwapChainGraphicsHost CreateSwapChainGraphicsHost(IntPtr handle, GraphicsDeviceProvider provider, bool useFlipModel)
        {
            return CreateSwapChainGraphicsHost(handle, provider, preferSwapChain1: true, useFlipModel: useFlipModel);
        }

        public static SwapChainGraphicsHost CreateSwapChainGraphicsHost(IntPtr handle, GraphicsDeviceProvider provider,
            bool preferSwapChain1, bool useFlipModel)
        {
            if (preferSwapChain1 && provider.DXGIFactory is DXGIFactory2)  //支援 DXGI 1.1
            {
                try
                {
                    return new SwapChainGraphicsHost1(provider, handle,
                        D2D1TextAntialiasMode.Grayscale,
                        useFlipModel);
                }
                catch (Exception)
                {
                }
            }
            return new SwapChainGraphicsHost(provider, handle,
                D2D1TextAntialiasMode.Grayscale,
                useFlipModel);
        }

        public static SwapChainGraphicsHost FromAnotherSwapChainGraphicsHost(SwapChainGraphicsHost another, IntPtr handle)
        {
            if (another is SwapChainGraphicsHost1)
            {
                return new SwapChainGraphicsHost1(another, handle);
            }
            else
            {
                return new SwapChainGraphicsHost(another, handle);
            }
        }

        public static string[] EnumAdapters(GraphicsDeviceProvider provider)
        {
            DXGIFactory factory = provider.DXGIFactory;
            if (factory is null)
                return Array.Empty<string>();
            List<string> result = new List<string>();
            for (uint i = 0; i < Constants.AdapterEnumerationLimit; i++)
            {
                DXGIAdapter? adapter = factory.EnumAdapters(i, throwException: false);
                if (adapter is null)
                    break;
                DXGIAdapterDescription description = adapter.Description;
                if (description.VendorId != 5140) //Is not "Microsoft Basic Render Driver"
                    result.Add(description.Description.ToString());
                adapter.Dispose();
            }
            return result.ToArray();
        }
    }
}
