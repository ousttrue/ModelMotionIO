using D3D11;
using NLog;
using RenderingPipe;
using RenderingPipe.Commands;
using RenderingPipe.Resources;
using RenderingPipe.Resources.VertexBuffers;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;


namespace WpfViewer.Models
{
    static class Extensions
    {
        public static SharpDX.Vector3 ToSharpDX(this MMIO.Vector3 src)
        {
            return new SharpDX.Vector3(src.X, src.Y, src.Z);
        }
    }

    public class Scene
    {
        #region Logger
        static Logger Logger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }
        #endregion

        #region Rendering
        ShaderResource m_vs;
        ShaderResource m_ps;


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

                foreach(var mesh in
                Root
                    .Traverse()
                    .Select(x => x.Mesh)
                    .Where(x => x!=null && x.VertexBuffer!=null))
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
                yield return ShaderVariableSetCommand.Create("view", m_orbitTransformation.Matrix);
                yield return ShaderVariableSetCommand.Create("projection", m_projection.Matrix);

                foreach (var mesh in Root
                    .Traverse()
                    .Select(x => x.Mesh)
                    .Where(x => x != null && x.VertexBuffer != null))
                {
                    if (mesh.VertexBufferUpdate != null) {
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

        public Scene()
        {
            BackgroundColor = RenderingPipe.Color4.Green;
            // camera
            m_orbitTransformation = new OrbitTransformation();
            m_projection = new PerspectiveProjection();
            // shader
            m_vs = ShaderResourceFactory.CreateFromSource(ShaderStage.Vertex, ShaderResourceFactory.WVP_SHADER);
            m_ps = ShaderResourceFactory.CreateFromSource(ShaderStage.Pixel, ShaderResourceFactory.WVP_SHADER);

            // Timer駆動でPushする
            Observable.Interval(TimeSpan.FromMilliseconds(33))
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
        #endregion

        #region LoadScene
        Node m_root;
        public Node Root
        {
            get {
                if(m_root==null)
                {
                    m_root = new Node();
                    Clear();
                }
                return m_root;
            }
        }

        public ObservableCollection<Node> Nodes
        {
            get
            {
                return Root.Children;
            }
        }

        void AddModel(Node model)
        {
            Root.Children.Add(model);

            var lines = Root.TraversePair().Select(x => new { Parent = x.Item1.Position, Offset = x.Item2.Offset });

            var vertices =
                from l in lines
                from v in new Vector3[] { l.Parent, l.Parent + l.Offset }
                select new Single[] { v.X, v.Y, v.Z, 1.0f, /*color*/ 1.0f, 1.0f, 1.0f, 1.0f, }
                ;

            var indices = lines.SelectMany((x, i) => new[] { i * 2, i * 2 + 1 });

            model.Mesh = new Mesh
            {
                VertexBuffer= VertexBufferResource.Create(vertices, indices),
            };
            model.Mesh.VertexBuffer.Topology = VertexBufferTopology.Lines;
        }

        public void Clear()
        {
            Root.Children.Clear();

            var grid = GridVertexBuffer.Create(10);
            Root.Children.Add(new Node
            {
                Name = "grid",
                Mesh = new Mesh
                {
                    VertexBuffer = grid,
                },
            });

            var axis = AxisVertexBuffer.Create(10);
            Root.Children.Add(new Node
            {
                Name = "axis",
                Mesh = new Mesh
                {
                    VertexBuffer = axis,
                },
            });

            Logger.Info("Clear");
        }

        public void SetMotion(Motion motion)
        {
            foreach (var node in Root.Traverse())
            {
                Curve curve;
                if (motion != null
                    && !String.IsNullOrEmpty(node.Name)
                    && motion.TryGetValue(node.Name, out curve))
                {
                    node.Curve = curve;
                }
                else
                {
                    node.Curve = null;
                }
            }
        }

        public void SetPose(Pose pose)
        {
            foreach (var node in Root.Children)
            {
                node.SetPose(pose);
            }
        }

        public void LoadPmd(Uri uri, Single scale=1.58f/20.0f)
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
                    Position = x.Position.ToSharpDX() * scale,
                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.Parent.HasValue ? nodes[x.Parent.Value] : root;
                node.Offset = node.Position - parent.Position;
                parent.Children.Add(node);
            });

            AddModel(root);
            Logger.Info("Loaded: {0}", uri);
        }

        public void LoadPmx(Uri uri, Single scale = 1.58f / 20.0f)
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
                    Position = x.Position.ToSharpDX() * scale,

                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) =>
            {
                var node = nodes[i];
                var parent = x.ParentIndex.HasValue ? nodes[x.ParentIndex.Value] : root;
                node.Offset = node.Position - parent.Position;
                parent.Children.Add(node);
            });

            AddModel(root);
            Logger.Info("Loaded: {0}", uri);
        }

        void BuildBvh(MMIO.Bvh.Node bvh, Node parent, Single scale)
        {
            var node = new Node
            {
                Name = bvh.Name,
                Offset = bvh.Offset.ToSharpDX() * scale,
            };
            node.Position = parent.Position + node.Offset;

            parent.Children.Add(node);

            foreach (var child in bvh.Children)
            {
                BuildBvh(child, node, scale);
            }
        }

        public void LoadBvh(Uri uri, Single scale=0.01f)
        {
            var root = new Node
            {
                Name = uri.ToString(),
            };
            var text = File.ReadAllText(uri.LocalPath);
            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);

            BuildBvh(bvh.Root, root, scale);

            AddModel(root);
            Logger.Info("Loaded: {0}", uri);
        }
        #endregion
    }
}
