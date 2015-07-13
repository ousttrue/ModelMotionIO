using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources.VertexBuffers
{
    public static class AxisVertexBuffer
    {
        class Vec4
        {
            public Vec4(float x, float y, float z, float w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
            public float X;
            public float Y;
            public float Z;
            public float W;
        }
        struct Vertex
        {
            public Vec4 Position;
            public Vec4 Color;
            public Single[] ToArray()
            {
                return new[] {
                    Position.X, Position.Y, Position.Z, Position.W
                    , Color.X, Color.Y, Color.Z, Color.W
                };
            }
        }

        public static VertexBufferResource Create(float n)
        {
            var vertices = new List<Vertex>();

            // x
            vertices.Add(new Vertex
            {
                Position=new Vec4(0, 0, 0, 1.0f),
                Color=new Vec4(1.0f, 0, 0, 1.0f),
            });
            vertices.Add(new Vertex
            {
                Position = new Vec4(n, 0, 0, 1.0f),
                Color = new Vec4(1.0f, 0, 0, 1.0f),
            });

            // y
            vertices.Add(new Vertex
            {
                Position = new Vec4(0, 0, 0, 1.0f),
                Color = new Vec4(0, 1.0f, 0, 1.0f),
            });
            vertices.Add(new Vertex
            {
                Position = new Vec4(0, n, 0, 1.0f),
                Color = new Vec4(0, 1.0f, 0, 1.0f),
            });

            // z
            vertices.Add(new Vertex
            {
                Position = new Vec4(0, 0, 0, 1.0f),
                Color = new Vec4(0, 0, 1.0f, 1.0f),
            });
            vertices.Add(new Vertex
            {
                Position = new Vec4(0, 0, n, 1.0f),
                Color = new Vec4(0, 0, 1.0f, 1.0f),
            });

            var vertexbuffer = VertexBufferResource.Create(vertices.Select(x => x.ToArray()));
            vertexbuffer.Topology = VertexBufferTopology.Lines;
            return vertexbuffer;
        }
    }
}
