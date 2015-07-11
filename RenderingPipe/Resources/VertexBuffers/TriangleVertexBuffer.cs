using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources.VertexBuffers
{
    public static class TriangleVertexBuffer
    {
        public static VertexBufferResource Create()
        {
            return VertexBufferResource.Create(new[]{
                            new Single[]{0.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f},
                            new Single[]{0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f},
                            new Single[]{-0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f},
                        });
        }
    }
}
