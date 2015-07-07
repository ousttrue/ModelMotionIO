using System;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Renderer.Commands
{
    class RenderTargetClearCommand: IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.Clear_Color; }
        }

        public override string ToString()
        {
            return String.Format("ClearColor: {0}"
                , Color);
        }

        public UInt32 ResourceID
        {
            get;
            private set;
        }

        public SharpDX.Color4 Color
        {
            get;
            private set;
        }

        public RenderTargetClearCommand Create(TextureResource resource, SharpDX.Color4 color)
        {
            if (resource == null)
            {
                return null;
            }
            return new RenderTargetClearCommand
            {
                ResourceID = resource.ID,
                Color = color,
            };
        }
    }
}
