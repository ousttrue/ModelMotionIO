using D3D11;
using RenderingPipe;
using RenderingPipe.Commands;
using RenderingPipe.Resources;
using RenderingPipe.Resources.VertexBuffers;
using SharpDX;
using SharpDXScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;

namespace WpfViewer.Models
{
    public class Mesh
    {
        public VertexBufferResource VertexBuffer { get; set; }
        public VertexBufferUpdateCommand VertexBufferUpdate { get; set; }

        public void UpdateVertexBuffer(Node node)
        {
            var gray = new SharpDX.Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            var white = new SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            var red = new SharpDX.Vector4(1.0f, 0, 0, 1.0f);

            var vertices = node.TraversePair()
                .Select(x =>
                {
                    if (x.Item2.Content.IsSelected.Value)
                    {
                        return new { line = x, color = red };
                    }
                    else if (x.Item2.Content.KeyFrame.Value == Transform.Identity)
                    {
                        return new { line = x, color = gray };
                    }
                    else
                    {
                        return new { line = x, color = white };
                    }
                })
                .SelectMany(x => new SharpDX.Vector4[] {
                    new SharpDX.Vector4(x.line.Item1.Content.WorldTransform.Translation, 1.0f), x.color
                    , new SharpDX.Vector4(x.line.Item2.Content.WorldTransform.Translation, 1.0f), x.color
                })
                .SelectMany(x => new Single[] { x.X, x.Y, x.Z, x.W })
                .ToArray()
                ;

            if (!vertices.Any()) return;

            // ToDO: Meshごとにシェーダーを見るべし
            // ToDo: 解放されている？
            var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * vertices.Length);
            Marshal.Copy(vertices, 0, ptr, vertices.Length);

            VertexBufferUpdate = VertexBufferUpdateCommand.Create(VertexBuffer, ptr);
        }
    }

    public class RenderModel : Livet.NotificationObject
    {
        ShaderResource m_vs;
        ShaderResource m_ps;

        Mesh m_grid = new Mesh { VertexBuffer = GridVertexBuffer.Create(10) };
        Mesh m_axis = new Mesh { VertexBuffer = AxisVertexBuffer.Create(10) };

        Dictionary<Node, Mesh> m_meshMap = new Dictionary<Node, Mesh>();

        int m_frame;
        Subject<RenderFrame> m_renderFrameSubject = new Subject<RenderFrame>();
        public IObservable<RenderFrame> RenderFrameObservable
        {
            get
            {
                return m_renderFrameSubject;
            }
        }

        public RenderingPipe.Color4 BackgroundColor
        {
            get;
            set;
        }

        IEnumerable<IRenderResource> Resources
        {
            get
            {
                yield return m_vs;
                yield return m_ps;

                yield return m_grid.VertexBuffer;
                yield return m_axis.VertexBuffer;

                foreach (var mesh in
                m_scene.Root
                    .Traverse()
                    .Select(x => (Node)x)
                    .Where(x => m_meshMap.ContainsKey(x))
                    .Select(x => m_meshMap[x])
                    .Where(x => x != null && x.VertexBuffer != null))
                {
                    yield return mesh.VertexBuffer;
                }
            }
        }

        IEnumerable<IRenderCommand> Commands
        {
            get
            {
                //yield return BackbufferClearCommand.Create(new RenderingPipe.Color4(0.5f * ((Single)Math.Sin((m_frame++) * 0.1f) + 1.0f), 0.5f, 0, 1.0f));
                yield return BackbufferClearCommand.Create(BackgroundColor);
                yield return ShaderSetCommand.Create(m_vs);
                yield return ShaderSetCommand.Create(m_ps);

                var time = m_frame * 0.1f;
                //var WorldMatrix = Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f);
                var WorldMatrix = Matrix.Identity;
                yield return ShaderVariableSetCommand.Create("world", WorldMatrix);
                yield return ShaderVariableSetCommand.Create("view", m_scene.OrbitTransformation.Matrix);
                yield return ShaderVariableSetCommand.Create("projection", m_scene.Projection.Matrix);

                foreach (var mesh in m_scene.Root
                    .Traverse()
                    .Where(x => m_meshMap.ContainsKey((Node)x))
                    .Select(x => m_meshMap[(Node)x])
                    .Where(x => x.VertexBuffer != null)
                    .Concat(new Mesh[] { m_grid, m_axis })
                    )
                {
                    if (mesh.VertexBufferUpdate != null)
                    {
                        yield return mesh.VertexBufferUpdate;
                    }

                    yield return VertexBufferSetCommand.Create(mesh.VertexBuffer);
                    foreach (var c in mesh.VertexBuffer.SubMeshes.Select(s => ShaderDrawSubMeshCommand.Create(s)))
                    {
                        yield return c;
                    }
                }
            }
        }

        Scene m_scene;

        public RenderModel(Scene scene)
        {
            m_scene = scene;
            scene.ModelAdded += (o, e) =>
            {
                var mesh=AddModel(e.Model);
                e.Model.Content.PoseSet += (oo, ee) =>
                {
                    mesh.UpdateVertexBuffer(e.Model);
                };
            };
            scene.Cleared += (o, e) =>
            {
                m_meshMap.Clear();
            };

            BackgroundColor = RenderingPipe.Color4.Green;
            // shader
            m_vs = ShaderResourceFactory.CreateFromSource(ShaderStage.Vertex, ShaderResourceFactory.WVP_SHADER);
            m_ps = ShaderResourceFactory.CreateFromSource(ShaderStage.Pixel, ShaderResourceFactory.WVP_SHADER);

            // Timer駆動でPushする
            Observable.Interval(TimeSpan.FromMilliseconds(33))
                .ObserveOnDispatcher()
            .Subscribe(_ =>
            {

                m_renderFrameSubject.OnNext(new RenderFrame
                {
                    Resources = Resources.ToArray(),
                    Commands = Commands.ToArray(),
                });

            })
            ;
        }

        Mesh AddModel(Node model)
        {
            var lines = model.TraversePair().Select(x => new { Parent = x.Item1.Content.WorldPosition.Value, Offset = x.Item2.Content.LocalPosition.Value });

            var vertices =
                from l in lines
                from v in new Vector3[] { l.Parent, l.Parent + l.Offset }
                select new Single[] { v.X, v.Y, v.Z, 1.0f, /*color*/ 1.0f, 1.0f, 1.0f, 1.0f, }
                ;

            var indices = lines.SelectMany((x, i) => new[] { i * 2, i * 2 + 1 });

            var mesh = new Mesh
            {
                VertexBuffer = VertexBufferResource.Create(vertices, indices),
            };
            mesh.VertexBuffer.Topology = VertexBufferTopology.Lines;

            m_meshMap[model] = mesh;

            return mesh;
        }
    }
}
