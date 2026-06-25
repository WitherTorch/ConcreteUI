using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ConcreteUI.Graphics;
using ConcreteUI.Utils;

using WitherTorch.Common.Helpers;

namespace ConcreteUI.Windows;

partial class CoreWindow
{
    [StructLayout(LayoutKind.Auto)]
    protected ref struct WindowLayoutData
    {
        public Rectangle MinimizeButtonBounds, MaximizeButtonBounds, CloseButtonBounds, PageBounds, TitleBarBounds;
        public Point DrawingOffset;
        public int ActiveBorderWidth;
    }

    [StructLayout(LayoutKind.Auto)]
    protected ref struct WindowRenderingData
    {
        public WindowLayoutData Layout;
        public ulong ResizeTimestamp, LastRenderTimestamp, CurrentRenderTimestamp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderInformation CreateRenderInformation(bool ignoreNeedRefresh)
            => new RenderInformation(ignoreNeedRefresh, ResizeTimestamp, LastRenderTimestamp, CurrentRenderTimestamp);
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct BatchUpdateScope : IDisposable
    {
        private RenderingController? _controller;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BatchUpdateScope(RenderingController controller)
        {
            controller.Lock();
            _controller = controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            _controller = null;
            controller.RequestUpdate(false);
            controller.Unlock();
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct CriticalUpdateScope : IDisposable
    {
        private RenderingController? _controller;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CriticalUpdateScope(RenderingController controller)
        {
            controller.Lock();
            controller.WaitForRendering();
            _controller = controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            _controller = null;
            controller.RequestUpdate(false);
            controller.Unlock();
        }
    }
}
