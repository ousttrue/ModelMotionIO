using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    public class PmdHeader
    {
        public Single Version { get; set; }
        public String Name { get; set; }
        public String Comment { get; set; }
    }

    public struct PmdVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Int16 BoneIndex0;
        public Int16 BoneIndex1;
        public Byte BoneWeight0;
        public Byte Flag;
    }

    public struct PmdMaterial
    {
        public Vector4 DiffuseRGBA;
        public Single Specularity;
        public Vector3 SpecularRGB;
        public Vector3 AmbientRGB;
        public Byte ToonIndex;
        public Byte Flag;
        public Int32 FaceIndexCount;
        public String TextureFile;
    }

    public class PmdModel
    {
        public PmdHeader Header { get; set; }
        public PmdVertex[] Vertices { get; set; }
        public UInt16[] Indices { get; set; }
        public PmdMaterial[] Materials { get; set; }
    }
}
