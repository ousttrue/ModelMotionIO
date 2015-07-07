using System;

namespace WpfViewer.Renderer.Resources
{
    class TextureResource: RenderResourceBase
    {
        public override RenderResourceType RenderResourceType
        {
            get { return RenderResourceType.Texture; }
        }

        public override string ToString()
        {
            if (UseRenderTargetView)
            {
                return "RenderTarget";
            }
            else if (UseDepthStencilView)
            {
                return "DepthStencil";
            }
            else
            {
                return "Texture";
            }
        }

        public Boolean UseRenderTargetView { get; set; }
        public Boolean UseDepthStencilView { get; set; }
        public Boolean UseD2DTarget { get; set; }

        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        public Byte[] Buffer { get; set; }

        public static TextureResource CreateRenderTarget()
        {
            return new TextureResource
            {
                UseRenderTargetView = true,
            };
        }

        public static TextureResource CreateDepthStencil()
        {
            return new TextureResource
            {
                UseDepthStencilView = true,
            };
        }

        public static TextureResource CreateD2DTarget(int w, int h)
        {
            return new TextureResource
            {
                UseD2DTarget = true,
                Width = w,
                Height = h,
            };
        }

        public static TextureResource Create(int w, int h, Byte[] buffer)
        {
            return new TextureResource
            {
                Width = w,
                Height = h,
                Buffer = buffer,
            };
        }
    }
}
