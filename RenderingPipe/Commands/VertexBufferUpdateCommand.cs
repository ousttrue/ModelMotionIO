using RenderingPipe.Resources;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RenderingPipe.Commands
{
    public class VertexBufferUpdateCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.VertexBuffer_Update; }
        }

        public override string ToString()
        {
            return String.Format("UpdateVertexBuffer");
        }

        public UInt32 ResourceID
        {
            get;
            private set;
        }

        /*
        public Byte[] Bytes
        {
            get;
            private set;
        }

        public static VertexBufferUpdateCommand Create(VertexBufferResource resource, Byte[] bytes)
        {
            if (resource == null)
            {
                return null;
            }

            return new VertexBufferUpdateCommand
            {
                ResourceID = resource.ID,
                Bytes=bytes,
            };
        }
        */

        public IntPtr Ptr
        {
            get;
            private set;
        }

        /*
        public Int32 Stride
        {
            get;
            private set;
        }

        public Int32 Size
        {
            get;
            private set;
        }
        */

        public static VertexBufferUpdateCommand Create(VertexBufferResource resource, IntPtr ptr)
        {
            if (resource == null)
            {
                return null;
            }

            return new VertexBufferUpdateCommand
            {
                ResourceID = resource.ID,
                Ptr = ptr,
                //Size=size,
                //Stride=stride,
            };
        }
    }
}
