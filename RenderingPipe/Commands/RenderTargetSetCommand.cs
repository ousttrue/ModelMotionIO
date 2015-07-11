using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderingPipe.Resources;

namespace RenderingPipe.Commands
{
    public class RenderTargetsSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.RenderTargets_Set; }
        }

        public override string ToString()
        {
            return String.Format("RenderTarget[{0}] DepthStencil[{1}]"
                , RenderTargets.Length, DepthStencil != 0);
        }

        public UInt32 DepthStencil
        {
            get;
            private set;
        }

        public UInt32[] RenderTargets
        {
            get;
            private set;
        }

        public static RenderTargetsSetCommand Create(TextureResource depthStencil, IEnumerable<TextureResource> rendertargets)
        {
            if (!rendertargets.Any())
            {
                return null;
            }
            return new RenderTargetsSetCommand
            {
                DepthStencil = depthStencil != null ? depthStencil.ID : 0,
                RenderTargets = rendertargets.Select(r => r.ID).ToArray(),
            };
        }
    }
}
