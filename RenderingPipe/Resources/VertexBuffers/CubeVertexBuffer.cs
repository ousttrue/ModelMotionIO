using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources.VertexBuffers
{
    public static class CubeVertexBuffer
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
        public static VertexBufferResource Create()
        {
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

            return VertexBufferResource.Create(vertices.Select(x => x.Position.value.Concat(x.Color.value).ToArray()));
        }
    }
}
