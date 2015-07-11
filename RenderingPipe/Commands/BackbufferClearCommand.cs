using System;

namespace RenderingPipe.Commands
{
    public class BackbufferClearCommand : IRenderCommand
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

        public Color4 Color
        {
            get;
            private set;
        }

        public static BackbufferClearCommand Create(Color4 color)
        {
            return new BackbufferClearCommand
            {
                Color = color,
            };
        }

    }
}
