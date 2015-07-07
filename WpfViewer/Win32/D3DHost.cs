using System;

namespace Win32
{
    class D3DHost: EmptyHwnd
    {
        SharpDX.Direct3D11.Device1 D3DDevice;
        SharpDX.DXGI.Device2 DXGIDevice;
        SharpDX.DXGI.SwapChain1 SwapChain;

        void CreateDevice()
        {
            // d3d11
            var flags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
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

        void CreateSwapchain()
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
                SwapChain = new SharpDX.DXGI.SwapChain1(dxgiFactory, DXGIDevice, Hwnd, ref sc);
                dxgiFactory.MakeWindowAssociation(Hwnd, SharpDX.DXGI.WindowAssociationFlags.IgnoreAltEnter);
            }
        }

        void ResizeSwapchain(int w, int h)
        {
            if (SwapChain == null) return;

            var sdesc = SwapChain.Description;
            SwapChain.ResizeBuffers(sdesc.BufferCount
                , w, h
                , sdesc.ModeDescription.Format, sdesc.Flags);
        }

        void DestroySwapChain()
        {
            SwapChain.Dispose();
            SwapChain = null;
        }

        void DestroyDevice()
        { 
            DXGIDevice.Dispose();
            DXGIDevice = null;

            D3DDevice.Dispose();
            D3DDevice = null;
        }

        void Draw()
        {
            if (DXGIDevice == null)
            {
                CreateDevice();
            }
            if (SwapChain == null)
            {
                CreateSwapchain();
            }

            using (var texture = SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            using (var RTV = new SharpDX.Direct3D11.RenderTargetView(D3DDevice, texture))
            //using (
            {
                var context = D3DDevice.ImmediateContext;
                var desc = texture.Description;
                context.ClearRenderTargetView(RTV, new SharpDX.Color4(0, 0.5f, 0, 0.5f));
            }

            var flags = SharpDX.DXGI.PresentFlags.None;
            //flags|=SharpDX.DXGI.PresentFlags.DoNotWait;
            SwapChain.Present(0, flags, new SharpDX.DXGI.PresentParameters());
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            switch((WM)msg)
            {
                case WM.WM_PAINT:
                    {
                        PAINTSTRUCT ps;
                        var hdc = Import.BeginPaint(hwnd, out ps);
                        Import.EndPaint(hwnd, ref ps);

                        Draw();
                        handled = true;
                    }
                    return IntPtr.Zero;

                case WM.WM_SIZE:
                    {
                        ResizeSwapchain(lParam.Lo(), lParam.Hi());
                        handled = true;
                    }
                    break;

                case WM.WM_DESTROY:
                    {
                        DestroySwapChain();
                        DestroyDevice();
                    }
                    break;

                case WM.WM_ERASEBKGND:
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_KEYDOWN:
                    //EmitKeyDowned(wParam.ToInt32());
                    ///handled = true;
                    return IntPtr.Zero;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
