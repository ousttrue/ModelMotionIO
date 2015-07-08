using System;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Renderer.Commands
{
    public class VertexBufferSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.VertexBuffer_Set; }
        }

        public override string ToString()
        {
            return String.Format("SetVertexBuffer");
        }

        public UInt32 ResourceID
        {
            get;
            private set;
        }

        public static VertexBufferSetCommand Create(VertexBufferResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new VertexBufferSetCommand
            {
                ResourceID = resource.ID,
            };
        }
    }
}
