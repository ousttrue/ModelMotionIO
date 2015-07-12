using RenderingPipe;
using RenderingPipe.Commands;
using RenderingPipe.Resources;
using System;
using System.Linq;
using D3D11.Extensions;


namespace D3D11
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
        ShaderManager m_shaderManager = new ShaderManager();
        VertexBufferManager m_vertexBufferManager = new VertexBufferManager();
        VertexBuffer m_vertexBuffer;
        class ShaderPass
        {
            public VertexShaderStage VertexShader { get; set; }
            public GeometryShaderStage GeometryShader { get; set; }
            public PixelShaderStage PixelShader { get; set; }

            public void Apply(SharpDX.Direct3D11.DeviceContext context)
            {
                if (VertexShader != null)
                {
                    if (VertexShader.ConstantBufferSlots != null)
                    {
                        VertexShader.ConstantBufferSlots
                            .Where(slot => slot.BackingStore != null && slot.BackingStore.Buffer != null)
                            .ForEach(slot => {
                                var data = new SharpDX.DataBox(slot.BackingStore.Buffer.Ptr);
                                context.UpdateSubresource(data, slot.Buffer);
                                context.VertexShader.SetConstantBuffer(slot.Index, slot.Buffer);
                            });
                    }
                }

                if (GeometryShader != null)
                {
                    if (GeometryShader.ConstantBufferSlots != null)
                    {
                        GeometryShader.ConstantBufferSlots
                            .Where(slot => slot.BackingStore != null && slot.BackingStore.Buffer != null)
                            .ForEach(slot =>
                            {
                                var data = new SharpDX.DataBox(slot.BackingStore.Buffer.Ptr);
                                context.UpdateSubresource(data, slot.Buffer);
                                context.GeometryShader.SetConstantBuffer(slot.Index, slot.Buffer);
                            });
                    }
                }

                if (PixelShader != null)
                {
                    if (PixelShader.ConstantBufferSlots != null)
                    {
                        PixelShader.ConstantBufferSlots
                            .Where(slot => slot.BackingStore != null && slot.BackingStore.Buffer != null)
                            .ForEach(slot =>
                            {
                                var data = new SharpDX.DataBox(slot.BackingStore.Buffer.Ptr);
                                context.UpdateSubresource(data, slot.Buffer);
                                context.PixelShader.SetConstantBuffer(slot.Index, slot.Buffer);
                            });
                    }
                }

                /*
                // set textures
                Textures.ForEach((t, i) => context.PixelShader.SetShaderResource(i, t.ShaderResourceView));

                // sampler
                Samplers.ForEach((s, i) => context.PixelShader.SetSampler(i, s.State));
                */
            }

            public void SetConstantVariable(ShaderVariableSetCommand command)
            {
                if (VertexShader != null)
                {
                    if (VertexShader.SetConstantVariable(command))
                    {
                        return;
                    }
                }
                if (GeometryShader != null)
                {
                    if (GeometryShader.SetConstantVariable(command))
                    {
                        return;
                    }
                }
                if (PixelShader != null)
                {
                    if (PixelShader.SetConstantVariable(command))
                    {
                        return;
                    }
                }
            }

            //m_psBuffer.Textures = m_resources.TextureManager.Get(command.Textures);

            public void SetSRV(SharpDX.Direct3D11.DeviceContext context, String key, SharpDX.Direct3D11.ShaderResourceView srv)
            {
                if (VertexShader != null)
                {
                    var slot = VertexShader.GetSRVSlot(key);
                    if (slot != null)
                    {
                        context.VertexShader.SetShaderResource(slot.Index, srv);
                        return;
                    }
                }
                if (GeometryShader != null)
                {
                    var slot = GeometryShader.GetSRVSlot(key);
                    if (slot != null)
                    {
                        context.GeometryShader.SetShaderResource(slot.Index, srv);
                        return;
                    }
                }
                if (PixelShader != null)
                {
                    var slot = PixelShader.GetSRVSlot(key);
                    if (slot != null)
                    {
                        context.PixelShader.SetShaderResource(slot.Index, srv);
                        return;
                    }
                }
            }
            public void SetSampler(ShaderSamplerSetCommand command)
            {

            }
        }
        ShaderPass m_pass;

        public void Render(RenderFrame frame)
        {
            if (SwapChain == null) return;

            /////////////////////////////////////////////////
            // update
            /////////////////////////////////////////////////
            foreach (var resource in frame.Resources)
            {
                switch (resource.RenderResourceType)
                {
                    case RenderResourceType.Texture:
                        //TextureManager.Ensure(device, resource as TextureResource);
                        break;

                    case RenderResourceType.Sampler:
                        //SamplerManager.Ensure(device, resource as SamplerResource);
                        break;

                    case RenderResourceType.Shader:
                        m_shaderManager.Ensure(D3DDevice, resource as ShaderResource);
                        break;

                    case RenderResourceType.VertexBuffer:
                        m_vertexBufferManager.Ensure(D3DDevice, resource as VertexBufferResource);
                        break;

                    case RenderResourceType.BlendState:
                        //BlendStateManager.Ensure(device, resource as BlendStateResource);
                        break;

                    case RenderResourceType.DepthStencilState:
                        //DepthStencilStateManager.Ensure(device, resource as DepthStencilStateResource);
                        break;

                    default:
                        throw new NotImplementedException(resource.ToString());
                }
            }

            /////////////////////////////////////////////////
            // render
            /////////////////////////////////////////////////
            m_pass = new ShaderPass();
            using (Backbuffer = SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            using (RTV = new SharpDX.Direct3D11.RenderTargetView(D3DDevice, Backbuffer))
            {
                var context = D3DDevice.ImmediateContext;
                foreach (var c in frame.Commands)
                {
                    switch (c.RenderCommandType)
                    {
                        /*
                        case RenderCommandType.RenderTargets_Set:
                            {
                                var command = c as RenderTargetsSetCommand;
                                var depthStencil = m_resources.TextureManager.Get(command.DepthStencil);
                                var renderTargets = m_resources.TextureManager.Get(command.RenderTargets);
                                context.OutputMerger.SetTargets(
                                    depthStencil != null ? depthStencil.DepthStencilView : null
                                    , renderTargets.Select(r => r.RenderTargetView).ToArray());

                            }
                            break;

                        case RenderCommandType.Viewport_Set:
                            {
                                var command = c as ViewportSetCommand;
                                context.Rasterizer.SetViewports(new SharpDX.ViewportF[] { command.Viewport });
                            }
                            break;
                            */

                        case RenderCommandType.Clear_Backbuffer:
                            {
                                var command = c as BackbufferClearCommand;
                                context.ClearRenderTargetView(RTV, command.Color.ToSharpDX());
                                context.OutputMerger.SetTargets((SharpDX.Direct3D11.DepthStencilView)null, new SharpDX.Direct3D11.RenderTargetView[] { RTV });
                                context.Rasterizer.SetViewports(new [] {
                                    new SharpDX.Mathematics.Interop.RawViewportF
                                    {
                                        X=0,
                                        Y=0,
                                        Width=Backbuffer.Description.Width,
                                        Height=Backbuffer.Description.Height,
                                    }
                                });
                            }
                            break;

                        /*
                    case RenderCommandType.Clear_Color:
                        {
                            var command = c as RenderTargetClearCommand;
                            var renderTarget = m_resources.TextureManager.Get(command.ResourceID);
                            if (renderTarget != null)
                            {
                                context.ClearRenderTargetView(renderTarget.RenderTargetView, command.Color);
                            }
                        }
                        break;

                    case RenderCommandType.Clear_Depth:
                        {
                            var command = c as DepthStencilClearCommand;
                            var depthStencil = m_resources.TextureManager.Get(command.ResourceID);
                            if (depthStencil != null)
                            {
                                context.ClearDepthStencilView(depthStencil.DepthStencilView
                                    , SharpDX.Direct3D11.DepthStencilClearFlags.Depth
                                    , command.Depth, command.Stencil);
                            }
                        }
                        break;
                        */

                        case RenderCommandType.VertexBuffer_Update:
                        {
                            var command = c as VertexBufferUpdateCommand;
                            var vertexBuffer = m_vertexBufferManager.Get(command.ResourceID);
                            if (vertexBuffer != null)
                            {
                                if (command.Ptr != IntPtr.Zero)
                                {
                                    var data = new SharpDX.DataBox(command.Ptr);
                                    context.UpdateSubresource(data, vertexBuffer.Vertices);
                                }
                            }
                        }
                        break;

                        case RenderCommandType.VertexBuffer_Set:
                            {
                                var command = c as VertexBufferSetCommand;
                                var vertexBuffer = m_vertexBufferManager.Get(command.ResourceID);
                                if (vertexBuffer != null)
                                {
                                    context.InputAssembler.PrimitiveTopology = vertexBuffer.Topology;

                                    context.InputAssembler.SetVertexBuffers(0,
                                        new SharpDX.Direct3D11.VertexBufferBinding(vertexBuffer.Vertices
                                            , vertexBuffer.Stride, 0));
                                    if (vertexBuffer.Indices != null)
                                    {
                                        context.InputAssembler.SetIndexBuffer(vertexBuffer.Indices, SharpDX.DXGI.Format.R32_UInt, 0);
                                    }

                                    m_vertexBuffer = vertexBuffer;
                                }
                            }
                            break;

                        case RenderCommandType.ShaderVriable_Set:
                            {
                                var command = c as ShaderVariableSetCommand;
                                m_pass.SetConstantVariable(command);
                            }
                            break;

                        /*
                    case RenderCommandType.ShaderTexture_Set:
                        {
                            var command = c as ShaderTextureSetCommand;
                            var texture = m_resources.TextureManager.Get(command.ResourceID);
                            if (texture != null)
                            {
                                m_pass.SetSRV(context, command.Key, texture.ShaderResourceView);
                            }
                        }
                        break;

                    case RenderCommandType.ShaderSampler_Set:
                        {
                            var command = c as ShaderSamplerSetCommand;
                            Pass.SetSampler(command);
                        }
                        break;
                        */

                        case RenderCommandType.Shader_Set:
                            {
                                var command = c as ShaderSetCommand;
                                switch (command.ShaderStage)
                                {
                                    case ShaderStage.Vertex:
                                        {
                                            var vertexShader = m_shaderManager.Get(command.ResourceID) as VertexShaderStage;
                                            if (vertexShader != null)
                                            {
                                                context.VertexShader.Set(vertexShader.Shader);
                                                //context.InputAssembler.InputLayout = vertexShader.VertexLayout;
                                                m_pass.VertexShader = vertexShader;
                                            }
                                        }
                                        break;

                                    case ShaderStage.Geometry:
                                        {
                                            var geometryShader = m_shaderManager.Get(command.ResourceID) as GeometryShaderStage;
                                            if (geometryShader != null)
                                            {
                                                context.GeometryShader.Set(geometryShader.Shader);
                                                m_pass.GeometryShader = geometryShader;
                                            }
                                        }
                                        break;

                                    case ShaderStage.Pixel:
                                        {
                                            var pixelShader = m_shaderManager.Get(command.ResourceID) as PixelShaderStage;
                                            if (pixelShader != null)
                                            {
                                                context.PixelShader.Set(pixelShader.Shader);
                                                m_pass.PixelShader = pixelShader;
                                            }
                                        }
                                        break;

                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            break;

                        case RenderCommandType.Shader_DrawSubMesh:
                            {
                                var command = c as ShaderDrawSubMeshCommand;
                                if (m_pass.VertexShader != null)
                                {
                                    context.InputAssembler.InputLayout = m_pass.VertexShader.VertexLayout;
                                    // 定数バッファの適用
                                    m_pass.Apply(context);

                                    if (m_vertexBuffer != null)
                                    {
                                        if (m_vertexBuffer.Indices != null)
                                        {
                                            context.DrawIndexed(command.Count, command.Offset, 0);
                                        }
                                        else
                                        {
                                            context.Draw(command.Count, command.Offset);
                                        }
                                    }
                                }
                            }
                            break;

                            /*
                        case RenderCommandType.BlendState_Set:
                            {
                                var command = c as BlendStateSetCommand;
                                var blendState = m_resources.BlendStateManager.Get(command.ResourceID);
                                if (blendState == null)
                                {
                                    return;
                                }
                                context.OutputMerger.SetBlendState(blendState.State, SharpDX.Color4.White);
                            }
                            break;

                        case RenderCommandType.DepthStencilState_Set:
                            {
                                var command = c as DepthStencilStateSetCommand;
                                var depthStencilState = m_resources.DepthStencilStateManager.Get(command.ResourceID);
                                if (depthStencilState == null)
                                {
                                    return;
                                }
                                context.OutputMerger.SetDepthStencilState(depthStencilState.State);
                            }
                            break;
                            */
                    }
                }
                context.Flush();
            }
            Backbuffer = null;
            RTV = null;

            /////////////////////////////////////////////////
            // flip
            /////////////////////////////////////////////////
            var flags = SharpDX.DXGI.PresentFlags.None;
            flags|=SharpDX.DXGI.PresentFlags.DoNotWait;
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
