using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WpfViewer.Renderer;
using WpfViewer.Renderer.Commands;
using WpfViewer.Renderer.Resources;


namespace WpfViewer.Models
{
    public interface IMouseEventObserver: IObserver<System.Windows.Input.MouseEventArgs>
    {

    }

    public class Scene: IMouseEventObserver
    {
        public class BaseMatrix
        {
            Matrix m_matrix = Matrix.Identity;
            public Matrix Matrix
            {
                get { return m_matrix; }
                set
                {
                    if (m_matrix == value)
                    {
                        return;
                    }
                    m_matrix = value;
                    EmitMatrixChanged();
                }
            }
            public event EventHandler MatrixChanged;
            void EmitMatrixChanged()
            {
                var tmp = MatrixChanged;
                if (tmp != null)
                {
                    tmp(this, EventArgs.Empty);
                }
            }
        }

        public class Transformation : BaseMatrix
        {
            void CalcMatrix()
            {
                Matrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Scaling, Vector3.Zero, Rotation, Translation);
            }

            Vector3 m_translation = Vector3.Zero;
            public Vector3 Translation
            {
                get { return m_translation; }
                set
                {
                    if (m_translation == value)
                    {
                        return;
                    }
                    m_translation = value;
                    CalcMatrix();
                }
            }

            Quaternion m_rotation = Quaternion.Identity;
            public Quaternion Rotation
            {
                get { return m_rotation; }
                set
                {
                    if (m_rotation == value)
                    {
                        return;
                    }
                    m_rotation = value;
                    CalcMatrix();
                }
            }

            Vector3 m_scaling = Vector3.One;
            public Vector3 Scaling
            {
                get { return m_scaling; }
                set
                {
                    if (m_scaling == value)
                    {
                        return;
                    }
                    m_scaling = value;
                    CalcMatrix();
                }
            }
        }

        public class OrbitView : Transformation
        {
            public OrbitView()
            {
                CalcView();
            }

            double m_shiftX = 0;
            double m_shiftY = 0;
            public void AddShift(double x, double y)
            {
                m_shiftX += x * m_distance;
                m_shiftY += y * m_distance;
                CalcView();
            }

            double m_distance = 10;
            public void Dolly(double d)
            {
                m_distance *= d;
                CalcView();
            }

            double m_yawRadians = 0;
            public void AddYaw(double rad)
            {
                m_yawRadians += rad;
                CalcView();
            }

            double m_pitchRadians = 0;
            public void AddPitch(double rad)
            {
                m_pitchRadians += rad;
                CalcView();
            }
            double m_rollRadians = 0;

            public void CalcView()
            {
                //Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
                Matrix = Matrix.RotationY((float)m_yawRadians)
                    * Matrix.RotationX((float)m_pitchRadians)
                    * Matrix.Translation((float)m_shiftX, (float)m_shiftY, (float)m_distance)
                    ;
            }
        }

        public class PerspectiveProjection
        {
            public PerspectiveProjection()
            {
                CalcProjection();
            }

            public Matrix Matrix
            {
                get;
                private set;
            }

            double m_zNear = 0.1;
            public double ZNear
            {
                get { return m_zNear; }
                set
                {
                    m_zNear = value;
                    CalcProjection();
                }
            }

            double m_zFar = 100.0;
            public double ZFar
            {
                get { return m_zFar; }
                set
                {
                    m_zFar = value;
                    CalcProjection();
                }
            }

            double m_fovYRadians = Math.PI / 4.0;
            public double FovYRadians
            {
                get { return m_fovYRadians; }
                set
                {
                    m_fovYRadians = value;
                    CalcProjection();
                }
            }

            Vector2 m_size;
            public Vector2 TargetSize
            {
                get { return m_size; }
                set
                {
                    if (m_size == value)
                    {
                        return;
                    }
                    m_size = value;
                    CalcProjection();
                }
            }

            public void CalcProjection()
            {
                if (TargetSize.Y == 0)
                {
                    Matrix = Matrix.Identity;
                    return;
                }

                Matrix = Matrix.PerspectiveFovLH((float)FovYRadians
                    , (float)TargetSize.X / (float)TargetSize.Y
                    , (float)ZNear, (float)ZFar);
            }
        }

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
        class Vec4: Vec3
        {
            public Vec4(float x, float y, float z, float w)
            {
                value = new []{ x, y, z, w };
            }
        }
        struct Vertex
        {
            public Vec3 Position;
            public Vec4 Color;
            public float[][] value
            {
                get {
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
                        Resources=Resources.ToArray(),
                        Commands=Commands.ToArray(),
                    });
                   
                })
                ;
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
        Matrix ViewMatrix=Matrix.Identity;

        IEnumerable<IRenderCommand> Commands
        {
            get
            {              
                yield return BackbufferClearCommand.Create(new Color4(0.5f * ((Single)Math.Sin((counter++) * 0.1f)+1.0f) , 0.5f, 0, 1.0f));
                yield return ShaderSetCommand.Create(m_vs);
                yield return ShaderSetCommand.Create(m_ps);

                var time = counter * 0.1f;
                var WorldMatrix=Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f);
                yield return ShaderVariableSetCommand.Create("world", WorldMatrix);

                yield return ShaderVariableSetCommand.Create("view", ViewMatrix);

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
                Name=uri.ToString(),
            };
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node
                {
                    Name=x.Name,

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

        public void OnNext(System.Windows.Input.MouseEventArgs value)
        {
            var button = value as System.Windows.Input.MouseButtonEventArgs;
            if (button != null) {
                return;
            }

            var wheel = value as System.Windows.Input.MouseWheelEventArgs;
            if (wheel != null)
            {
                return;
            }

            // move

        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }
    }
}
