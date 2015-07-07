using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Commands
{
    class BackbufferClearCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get
            {
                return RenderCommandType.Clear_Backbuffer;
            }
        }

        public override string ToString()
        {
            return String.Format("ClearColor: {0}"
                , Color);
        }

        public SharpDX.Color4 Color
        {
            get;
            private set;
        }

        public static BackbufferClearCommand Create(SharpDX.Color4 color)
        {
            return new BackbufferClearCommand
            {
                Color = color,
            };
        }

    }
}
