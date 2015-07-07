using System;
using WpfViewer.Renderer;
using WpfViewer.Renderer.Commands;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Win32.D3D11
{
    class D3D11Renderer : IRenderer, IDisposable
    {
        #region Device
        SharpDX.Direct3D11.Device1 D3DDevice;
        SharpDX.DXGI.Device2 DXGIDevice;
        SharpDX.DXGI.SwapChain1 SwapChain;
        SharpDX.Direct3D11.Texture2D Backbuffer;
        SharpDX.Direct3D11.RenderTargetView RTV;

        void CreateDevice()
        {
            // d3d11
            var flags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            // error on Windows10(Geforce435M)
            //flags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif

            using (var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware
                , flags
                , SharpDX.Direct3D.FeatureLevel.Level_10_0
                ))
            {
                D3DDevice = device.QueryInterfaceOrNull<SharpDX.Direct3D11.Device1>();
            }

            // DXGIDevice
            DXGIDevice = D3DDevice.QueryInterface<SharpDX.DXGI.Device2>();
            DXGIDevice.MaximumFrameLatency = 1;
        }

        public void OnPaint(IntPtr hwnd)
        {
            if (SwapChain != null) return;

            // 初回にWM_PAINTから呼ばれたときに初期化する
            CreateDevice();
            CreateSwapchain(hwnd);
        }

        void CreateSwapchain(IntPtr hwnd)
        {
            using (var a = DXGIDevice.Adapter)
            using (var adapter = a.QueryInterface<SharpDX.DXGI.Adapter1>())
            using (var dxgiFactory = adapter.GetParent<SharpDX.DXGI.Factory2>())
            {
                var sc = new SharpDX.DXGI.SwapChainDescription1
                {
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    Scaling = SharpDX.DXGI.Scaling.Stretch,
                    AlphaMode = SharpDX.DXGI.AlphaMode.Unspecified,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                    //SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                    SampleDescription = new SharpDX.DXGI.SampleDescription
                    {
                        Count = 1,
                        Quality = 0,
                    },
                    Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
                };
                SwapChain = new SharpDX.DXGI.SwapChain1(dxgiFactory, DXGIDevice, hwnd, ref sc);
                dxgiFactory.MakeWindowAssociation(hwnd, SharpDX.DXGI.WindowAssociationFlags.IgnoreAltEnter);
            }
        }

        public void ResizeSwapchain(int w, int h)
        {
            if (SwapChain != null)
            {
                var sdesc = SwapChain.Description;
                SwapChain.ResizeBuffers(sdesc.BufferCount
                    , w, h
                    , sdesc.ModeDescription.Format, sdesc.Flags);
            }
        }
        #endregion

        #region IRenderer
        public void Render(RenderFrame frame)
        {
            if (SwapChain == null) return;

            // update
            foreach (var r in frame.Resources)
            {
            }

            using (Backbuffer = SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            using (RTV = new SharpDX.Direct3D11.RenderTargetView(D3DDevice, Backbuffer))
            {
                // render
                var context = D3DDevice.ImmediateContext;
                foreach (var c in frame.Commands)
                {
                    switch (c.RenderCommandType)
                    {
                        case RenderCommandType.Clear_Backbuffer:
                            {
                                var command = c as BackbufferClearCommand;
                                context.ClearRenderTargetView(RTV, command.Color);
                            }
                            break;
                    }
                }
            }
            Backbuffer = null;
            RTV = null;

            var flags = SharpDX.DXGI.PresentFlags.None;
            //flags|=SharpDX.DXGI.PresentFlags.DoNotWait;
            SwapChain.Present(0, flags, new SharpDX.DXGI.PresentParameters());
        }
        #endregion

        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (SwapChain != null)
                {
                    SwapChain.Dispose();
                    SwapChain = null;
                }

                // Free any other managed objects here.
                if (DXGIDevice != null)
                {
                    DXGIDevice.Dispose();
                    DXGIDevice = null;
                }

                if (D3DDevice != null)
                {
                    D3DDevice.Dispose();
                    D3DDevice = null;
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
        #endregion
    }
}
