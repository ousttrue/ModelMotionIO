using RenderingPipe.Resources;
using System;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Win32.D3D11
{
    public class VertexBuffer: ResourceItemBase
    {
        public SharpDX.Direct3D11.Buffer Vertices { get; set; }
        public Int32 Stride { get; set; }
        public SharpDX.Direct3D.PrimitiveTopology Topology { get; set; }

        public SharpDX.Direct3D11.Buffer Indices { get; set; }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Vertices != null)
                {
                    Vertices.Dispose();
                    Vertices = null;
                }
                if (Indices != null)
                {
                    Indices.Dispose();
                    Indices = null;
                }
            }
        }
        #endregion
    }


    public class VertexBufferManager : BaseResourceManager<VertexBuffer>
    {
        public VertexBuffer CreateVertexBuffer(SharpDX.Direct3D11.Device device
            , Byte[] bytes
            , Int32 stride
            , Int32[] indices
            , SharpDX.Direct3D.PrimitiveTopology topology)
        {
            if (bytes == null)
            {
                return null;
            }

            // vertex buffer
            var vertexStream = new SharpDX.DataStream(bytes.Length, true, true);

            var vdesc = new SharpDX.Direct3D11.BufferDescription
            {
                BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SizeInBytes = (int)vertexStream.Length,
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
            };

            SharpDX.Direct3D11.Buffer vbuffer;
            if (bytes.Length > 0)
            {
                vertexStream.WriteRange(bytes);
                vertexStream.Position = 0;
                vbuffer = new SharpDX.Direct3D11.Buffer(device, vertexStream, vdesc);
            }
            else
            {
                // emmpty vertex buffer for geometry shader;
                if (vdesc.SizeInBytes == 0)
                {
                    // ?
                    vdesc.SizeInBytes = 2;
                }
                vbuffer = new SharpDX.Direct3D11.Buffer(device, vertexStream, vdesc);
                topology = SharpDX.Direct3D.PrimitiveTopology.PointList;
            }

            var vertexBuffer = new VertexBuffer
            {
                Vertices = vbuffer,
                Stride = stride,
                Topology = topology,
            };

            // index buffer
            if (indices!=null && indices.Length > 0)
            {
                var indexStream = new SharpDX.DataStream(indices.Length * 4, true, true);
                indexStream.WriteRange(indices);
                indexStream.Position = 0;

                var idesc = new SharpDX.Direct3D11.BufferDescription
                {
                    BindFlags = SharpDX.Direct3D11.BindFlags.IndexBuffer,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SizeInBytes = (int)indexStream.Length,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                };
                var ibuffer = new SharpDX.Direct3D11.Buffer(device, indexStream, idesc);

                vertexBuffer.Indices = ibuffer;
            }

            return vertexBuffer;
        }

        public bool Ensure(SharpDX.Direct3D11.Device device, VertexBufferResource r)
        {
            if (Get(r.ID) != null)
            {
                return false;
            }

            SharpDX.Direct3D.PrimitiveTopology topology;
            switch(r.Topology)
            {
                case VertexBufferTopology.TriangleList:
                    topology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                    break;

                case VertexBufferTopology.Lines:
                    topology = SharpDX.Direct3D.PrimitiveTopology.LineList;
                    break;

                default:
                    throw new NotImplementedException();
            }

            var vertexBuffer = CreateVertexBuffer(device, r.Vertices, r.Stride, r.Indices, topology);
            if (vertexBuffer == null)
            {
                return false;
            }

            Add(r.ID, vertexBuffer);

            return true;
        }
    }
}
