using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Resources
{
    public enum RenderResourceType
    {
        Texture,
        Sampler,

        Shader,
        VertexBuffer,

        RasterizerState,
        BlendState,
        DepthStencilState,
    }
}
