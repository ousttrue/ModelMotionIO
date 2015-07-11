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
using WpfViewer.Renderer;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Models
{
    public class Scene
    {

        class Vec3
        {
            public Vec3()
            {

            }
            public Vec3(float x, float y, float z)
            {
                value = new[] { x, y, z, 1.0f };
            }
            public float[] value;
        }
        class Vec4 : Vec3
        {
            public Vec4(float x, float y, float z, float w)
            {
                value = new[] { x, y, z, w };
            }
        }
        struct Vertex
        {
            public Vec3 Position;
            public Vec4 Color;
            public float[][] value
            {
                get
                {
                    return new[] { Position.value, Color.value };
                }
            }
        }

        public Scene()
        {
            m_projection = new PerspectiveProjection();
#if triangle
            m_vs = ShaderResourceFactory.CreateFromSource(ShaderStage.Vertex, ShaderResourceFactory.THROUGH_SHADER);
            m_ps = ShaderResourceFactory.CreateFromSource(ShaderStage.Pixel, ShaderResourceFactory.THROUGH_SHADER);
            m_vertexbuffer = VertexBufferResource.Create(new[]{
                            new Single[]{0.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f},
                            new Single[]{0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f},
                            new Single[]{-0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f},
                        });
#else
            m_vs = ShaderResourceFactory.CreateFromSource(ShaderStage.Vertex, ShaderResourceFactory.WVP_SHADER);
            m_ps = ShaderResourceFactory.CreateFromSource(ShaderStage.Pixel, ShaderResourceFactory.WVP_SHADER);
            var vertices = new Vertex[]{
                new Vertex{Position=new Vec3(-1.0f, -1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)}, // Front
                new Vertex{Position=new Vec3(-1.0f,  1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,  1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f, -1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,  1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, -1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 0.0f, 1.0f)},

                new Vertex{Position=new Vec3(-1.0f, -1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)}, // BACK
                new Vertex{Position=new Vec3( 1.0f,  1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,  1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f, -1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, -1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,  1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 0.0f, 1.0f)},

                new Vertex{Position=new Vec3(-1.0f, 1.0f, -1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)}, // Top
                new Vertex{Position=new Vec3(-1.0f, 1.0f,  1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, 1.0f,  1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f, 1.0f, -1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, 1.0f,  1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, 1.0f, -1.0f), Color=new Vec4(0.0f, 0.0f, 1.0f, 1.0f)},

                new Vertex{Position=new Vec3(-1.0f,-1.0f, -1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)}, // Bottom
                new Vertex{Position=new Vec3( 1.0f,-1.0f,  1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,-1.0f,  1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,-1.0f, -1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,-1.0f, -1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,-1.0f,  1.0f), Color=new Vec4(1.0f, 1.0f, 0.0f, 1.0f)},

                new Vertex{Position=new Vec3(-1.0f, -1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)}, // Left
                new Vertex{Position=new Vec3(-1.0f, -1.0f,  1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,  1.0f,  1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f, -1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,  1.0f,  1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3(-1.0f,  1.0f, -1.0f), Color=new Vec4(1.0f, 0.0f, 1.0f, 1.0f)},

                new Vertex{Position=new Vec3( 1.0f, -1.0f, -1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)}, // Right
                new Vertex{Position=new Vec3( 1.0f,  1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, -1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f, -1.0f, -1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,  1.0f, -1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)},
                new Vertex{Position=new Vec3( 1.0f,  1.0f,  1.0f), Color=new Vec4(0.0f, 1.0f, 1.0f, 1.0f)},
            };

            m_vertexbuffer = VertexBufferResource.Create(vertices.Select(x => x.Position.value.Concat(x.Color.value).ToArray()));
#endif

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

            m_orbitTransformation = new OrbitTransformation();
        }

        ShaderResource m_vs;
        ShaderResource m_ps;
        VertexBufferResource m_vertexbuffer;
        PerspectiveProjection m_projection;

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

        int counter;

        OrbitTransformation m_orbitTransformation;
        public OrbitTransformation OrbitTransformation
        {
            get
            {
                return m_orbitTransformation;
            }
        }

        IEnumerable<IRenderCommand> Commands
        {
            get
            {
                yield return BackbufferClearCommand.Create(new RenderingPipe.Color4(0.5f * ((Single)Math.Sin((counter++) * 0.1f) + 1.0f), 0.5f, 0, 1.0f));
                yield return ShaderSetCommand.Create(m_vs);
                yield return ShaderSetCommand.Create(m_ps);

                var time = counter * 0.1f;
                //var WorldMatrix = Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f);
                var WorldMatrix = Matrix.Identity;
                yield return ShaderVariableSetCommand.Create("world", WorldMatrix);

                yield return ShaderVariableSetCommand.Create("view", m_orbitTransformation.Matrix);
                //yield return ShaderVariableSetCommand.Create("view", Matrix.Identity);

                yield return ShaderVariableSetCommand.Create("projection", m_projection.Matrix);
                yield return VertexBufferSetCommand.Create(m_vertexbuffer);
                foreach (var c in m_vertexbuffer.SubMeshes.Select(s => ShaderDrawSubMeshCommand.Create(s)))
                {
                    yield return c;
                }
            }
        }

        Subject<RenderFrame> m_renderFrameSubject = new Subject<RenderFrame>();
        public IObservable<RenderFrame> RenderFrameObservable
        {
            get
            {
                return m_renderFrameSubject;
            }
        }

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
    }
}
