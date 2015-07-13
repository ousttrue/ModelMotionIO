using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources.VertexBuffers
{
    public static class GridVertexBuffer
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

        public static VertexBufferResource Create(int n)
        {
            var color = new Vec4(0.5f, 0.5f, 0.5f, 1.0f);
            var vertices = new List<Vertex>();

            var minX = -n;
            var maxX = n;
            for (int z = -n; z <= n; ++z)
            {
                vertices.Add(new Vertex
                {
                    Position = new Vec4(minX, 0, z, 1.0f),
                    Color = color,
                });
                vertices.Add(new Vertex
                {
                    Position = new Vec4(maxX, 0, z, 1.0f),
                    Color = color,
                });
            }

            var minZ = -n;
            var maxZ = n;
            for (int x = -n; x <= n; ++x)
            {
                vertices.Add(new Vertex
                {
                    Position = new Vec4(x, 0, minZ, 1.0f),
                    Color = color,
                });
                vertices.Add(new Vertex
                {
                    Position = new Vec4(x, 0, maxZ, 1.0f),
                    Color = color,
                });
            }
            var grid = VertexBufferResource.Create(vertices.Select(x => x.ToArray()));

            grid.Topology = VertexBufferTopology.Lines;
            return grid;
        }
    }
}
