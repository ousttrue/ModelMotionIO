using System;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Renderer.Commands
{
    public class ShaderDrawSubMeshCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.Shader_DrawSubMesh; }
        }

        public Int32 Count
        {
            get;
            private set;
        }

        public Int32 Offset
        {
            get;
            private set;
        }

        public static ShaderDrawSubMeshCommand Create(VertexBufferResource.SubMesh submesh)
        {
            return new ShaderDrawSubMeshCommand
            {
                Count = submesh.Count,
                Offset = submesh.Offset,
            };
        }
    }
}
