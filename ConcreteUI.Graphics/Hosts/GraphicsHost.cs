using System;
using System.Collections.Generic;

using ConcreteUI.Graphics.Native.Direct2D;
using ConcreteUI.Graphics.Native.DXGI;

namespace ConcreteUI.Graphics.Hosts
{
    public static class GraphicsHost
    {
        public static IGraphicsHost Create(IntPtr handle, GraphicsDeviceProvider provider, bool useFlipModel, bool useDComp, bool isOpaque)
        {
            if (useDComp && useFlipModel)
                return new CompositionGraphicsHost(provider, handle, D2D1TextAntialiasMode.Grayscale, isOpaque); // 無法回退 (因為視窗的性質不一樣)

            if (provider.IsSupportSwapChain1) // 支援 DXGI 1.1
            {
                try
                {
                    return new OptimizedGraphicsHost(provider, handle,
                        D2D1TextAntialiasMode.Grayscale,
                        useFlipModel, isOpaque);
                }
                catch (Exception)
                {
                }
            }

            return new SwapChainGraphicsHost(provider, handle,
                D2D1TextAntialiasMode.Grayscale,
                useFlipModel, isOpaque);
        }

        public static IGraphicsHost FromAnother(IGraphicsHost another, IntPtr handle, bool isOpaque)
            => another switch
            {
                CompositionGraphicsHost typedAnother => new CompositionGraphicsHost(typedAnother, handle, isOpaque),
                OptimizedGraphicsHost typedAnother => new OptimizedGraphicsHost(typedAnother, handle, isOpaque),
                SwapChainGraphicsHost typedAnother => new SwapChainGraphicsHost(typedAnother, handle, isOpaque),
                HwndGraphicsHost typedAnother => new HwndGraphicsHost(typedAnother, handle, isOpaque),
                _ => throw new ArgumentException($"Unknown {nameof(IGraphicsHost)} implementation.", nameof(another)),
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
