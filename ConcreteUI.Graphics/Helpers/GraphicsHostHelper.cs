using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Hosts;
using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

namespace ConcreteUI.Graphics.Helpers
{
    public static class GraphicsHostHelper
    {
        public static SimpleGraphicsHost CreateSwapChainGraphicsHost(IntPtr handle, GraphicsDeviceProvider provider, bool useFlipModel, bool useDComp)
        {
            if (useDComp && useFlipModel)
                return new CompositionGraphicsHost(provider, handle, D2D1TextAntialiasMode.Grayscale); // 無法回退 (因為視窗的性質不一樣)

            if (provider.IsSupportSwapChain1) // 支援 DXGI 1.1
            {
                try
                {
                    return new OptimizedGraphicsHost(provider, handle,
                        D2D1TextAntialiasMode.Grayscale,
                        useFlipModel);
                }
                catch (Exception)
                {
                }
            }
            return new SimpleGraphicsHost(provider, handle,
                D2D1TextAntialiasMode.Grayscale,
                useFlipModel);
        }

        public static SimpleGraphicsHost FromAnotherSwapChainGraphicsHost(SimpleGraphicsHost another, IntPtr handle)
            => another switch
            {
                CompositionGraphicsHost typedAnother => new CompositionGraphicsHost(typedAnother, handle),
                OptimizedGraphicsHost typedAnother => new CompositionGraphicsHost(typedAnother, handle),
                _ => new SimpleGraphicsHost(another, handle)
            };

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
                if (description.VendorId != 5140) // Is not "Microsoft Basic Render Driver"
                    result.Add(description.Description.ToString());
                adapter.Dispose();
            }
            return result.ToArray();
        }
    }
}
