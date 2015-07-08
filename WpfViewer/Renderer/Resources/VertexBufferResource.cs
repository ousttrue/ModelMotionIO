using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Resources
{
    public enum VertexBufferTopology
    {
        TriangleList,
    }

    public class VertexBufferResource : RenderResourceBase
    {
        public override RenderResourceType RenderResourceType
        {
            get { return RenderResourceType.VertexBuffer; }
        }

        public VertexBufferTopology Topology;
        public Int32 Stride { get; set; }
        public Byte[] Vertices { get; set; }
        public Int32[] Indices { get; set; }

        public class SubMesh
        {
            public Int32 Count { get; set; }
            public Int32 Offset { get; set; }

            public SubMesh(int count, int offset = 0)
            {
                Count = count;
                Offset = offset;
            }
        }
        public SubMesh[] SubMeshes { get; set; }

        public static VertexBufferResource Create(IEnumerable<Single[]> vertices
            , IEnumerable<Int32> indices = null
            , IEnumerable<SubMesh> submeshes = null)
        {
            var ms = new MemoryStream();
            using (var w = new BinaryWriter(ms))
            {
                vertices.SelectMany(v => v)
                    .ForEach(n => w.Write(n));
            }

            if (submeshes == null)
            {
                if (indices == null)
                {
                    submeshes = new SubMesh[] { new SubMesh(vertices.Count()) };
                }
                else
                {
                    submeshes = new SubMesh[] { new SubMesh(indices.Count()) };
                }
            }

            return new VertexBufferResource
            {
                Vertices = ms.ToArray(),
                Stride = vertices.First().Length * 4,
                Indices = indices != null ? indices.ToArray() : null,
                SubMeshes = submeshes.ToArray(),
            };
        }

        public class VertexWriter<VERTEX>
        {
            public delegate void StreamWriter(SharpDX.DataStream w, VERTEX v);

            public StreamWriter Writer
            {
                get;
                set;
            }

            public Int32 Stride
            {
                get;
                set;
            }

            public VertexWriter(StreamWriter writer, Int32 stride)
            {
                Writer = writer;
                Stride = stride;
            }

            public Byte[] ToBytes(IEnumerable<VERTEX> vertices)
            {
                Byte[] buffer;
                using (var w = new SharpDX.DataStream(Stride * vertices.Count(), true, true))
                {
                    vertices.ForEach(v => Writer(w, v));
                    w.Position = 0;
                    buffer = new Byte[w.Length];
                    w.ReadRange(buffer, 0, buffer.Length);
                }
                return buffer;
            }
        }

        public static VertexBufferResource Create<T>(IEnumerable<T> vertices
            , VertexWriter<T> vertexWriter
            , IEnumerable<Int32> indices = null
            , IEnumerable<SubMesh> submeshes = null)
        {
            if (submeshes == null)
            {
                if (indices == null)
                {
                    submeshes = new[] { new SubMesh(vertices.Count()) };
                }
                else
                {
                    submeshes = new[] { new SubMesh(indices.Count()) };
                }
            }

            return new VertexBufferResource
            {
                Vertices = vertexWriter.ToBytes(vertices),
                Stride = vertexWriter.Stride,
                Indices = indices != null ? indices.ToArray() : null,
                SubMeshes = submeshes.ToArray(),
            };
        }

        public static VertexBufferResource Create<T>(IEnumerable<T> vertices
            , Func<T, Byte[]> toBytes, int vertexStride
            , IEnumerable<Int32> indices = null
            , IEnumerable<SubMesh> submeshes = null)
        {
            return Create(vertices.SelectMany(v => toBytes(v)).ToArray(), vertexStride, indices, submeshes);
        }

        public static VertexBufferResource Create(
            Byte[] vertices, int vertexStride
            , IEnumerable<Int32> indices = null
            , IEnumerable<SubMesh> submeshes = null)
        {
            if (submeshes == null)
            {
                if (indices == null)
                {
                    submeshes = new[] { new SubMesh(vertices.Count()) };
                }
                else
                {
                    submeshes = new[] { new SubMesh(indices.Count()) };
                }
            }

            return new VertexBufferResource
            {
                Vertices = vertices,
                Stride = vertexStride,
                Indices = indices != null ? indices.ToArray() : null,
                SubMeshes = submeshes.ToArray(),
            };
        }
    }
}
