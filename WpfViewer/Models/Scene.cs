using RenderingPipe;
using RenderingPipe.Commands;
using RenderingPipe.Resources;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Models
{
    public class Scene
    {
        #region Rendering
        ShaderResource m_vs;
        ShaderResource m_ps;
        VertexBufferResource m_vertexbuffer;

        PerspectiveProjection m_projection;
        public PerspectiveProjection Projection
        {
            get { return m_projection; }
        }

        OrbitTransformation m_orbitTransformation;
        public OrbitTransformation OrbitTransformation
        {
            get
            {
                return m_orbitTransformation;
            }
        }

        int m_frame;
        Subject<RenderFrame> m_renderFrameSubject = new Subject<RenderFrame>();
        public IObservable<RenderFrame> RenderFrameObservable
        {
            get
            {
                return m_renderFrameSubject;
            }
        }

        IEnumerable<IRenderResource> Resources
        {
            get
            {
                yield return m_vs;
                yield return m_ps;

                /*
                var lines = Root.Traverse((Node, pos) => new { Parent = pos, Offset = Node.Position });

                var vertices = 
                    from l in lines
                    from v in new Vector3[] { l.Parent, l.Parent + l.Offset }
                    select new Single[] { v.X, v.Y, v.Z }
                    ;

                var indices = lines.SelectMany((x, i) => new[] { i * 2, i * 2 + 1 });

                yield return VertexBufferResource.Create(vertices, indices);
                */
                yield return m_vertexbuffer;
            }
        }

        IEnumerable<IRenderCommand> Commands
        {
            get
            {
                yield return BackbufferClearCommand.Create(new RenderingPipe.Color4(0.5f * ((Single)Math.Sin((m_frame++) * 0.1f) + 1.0f), 0.5f, 0, 1.0f));
                yield return ShaderSetCommand.Create(m_vs);
                yield return ShaderSetCommand.Create(m_ps);

                var time = m_frame * 0.1f;
                //var WorldMatrix = Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f);
                var WorldMatrix = Matrix.Identity;
                yield return ShaderVariableSetCommand.Create("world", WorldMatrix);
                yield return ShaderVariableSetCommand.Create("view", m_orbitTransformation.Matrix);
                yield return ShaderVariableSetCommand.Create("projection", m_projection.Matrix);
                yield return VertexBufferSetCommand.Create(m_vertexbuffer);
                foreach (var c in m_vertexbuffer.SubMeshes.Select(s => ShaderDrawSubMeshCommand.Create(s)))
                {
                    yield return c;
                }
            }
        }

        public Scene()
        {
            // camera
            m_orbitTransformation = new OrbitTransformation();
            m_projection = new PerspectiveProjection();
            // shader
            m_vs = ShaderResourceFactory.CreateFromSource(ShaderStage.Vertex, ShaderResourceFactory.WVP_SHADER);
            m_ps = ShaderResourceFactory.CreateFromSource(ShaderStage.Pixel, ShaderResourceFactory.WVP_SHADER);
            // mesh
            m_vertexbuffer = RenderingPipe.Resources.VertexBuffers.CubeVertexBuffer.Create();

            // Timer駆動でPushする
            Observable.Interval(TimeSpan.FromMilliseconds(33))
            .Subscribe(_ => {

                m_renderFrameSubject.OnNext(new RenderFrame
                {
                    Resources = Resources.ToArray(),
                    Commands = Commands.ToArray(),
                });

            })
            ;
        }
        #endregion

        #region LoadScene
        public Node Root
        {
            get;
            set;
        }

        public Node LoadPmd(Uri uri)
        {
            var root = new Node
            {
                Name = uri.ToString(),
            };
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node
                {
                    Name = x.Name,

                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) => {
                var node = nodes[i];
                var parent = x.Parent.HasValue ? nodes[x.Parent.Value] : root;
                parent.Children.Add(node);
            });

            Root = root;
            return root;
        }

        public Node LoadPmx(Uri uri)
        {
            var root = new Node
            {
                Name = uri.ToString(),
            };
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmxParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node
                {
                    Name = x.Name,

                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) => {
                var node = nodes[i];
                var parent = x.ParentIndex.HasValue ? nodes[x.ParentIndex.Value] : root;
                parent.Children.Add(node);
            });

            Root = root;
            return root;
        }
        #endregion
    }
}
