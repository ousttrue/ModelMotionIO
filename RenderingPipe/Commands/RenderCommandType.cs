using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Commands
{
    public enum RenderCommandType
    {
        RenderTargets_Set,
        Viewport_Set,
        Clear_Backbuffer,
        Clear_Color,
        Clear_Depth,

        VertexBuffer_Update,
        VertexBuffer_Set,

        Shader_Set,
        ShaderVriable_Set,
        ShaderTexture_Set,
        ShaderSampler_Set,
        Shader_DrawSubMesh,

        Effect_Set,
        EffectVariable_Set,
        Effect_DrawSubMesh,

        BlendState_Set,
        DepthStencilState_Set,
    }
}
