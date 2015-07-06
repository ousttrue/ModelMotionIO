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
        public Int16? Bone0;
        public Int16? Bone1;
        public Byte BoneWeight0;
        public Byte Flag;
    }

    public class PmdMaterial
    {
        public Vector4 DiffuseRGBA { get; set; }
        public Single Specularity { get; set; }
        public Vector3 SpecularRGB { get; set; }
        public Vector3 AmbientRGB { get; set; }
        public Byte ToonIndex { get; set; }
        public Byte Flag { get; set; }
        public Int32 FaceIndexCount { get; set; }
        public String TextureFile { get; set; }
    }

    public enum PmdBoneType
    {
        RotationOnly,
        RotationAndTranslation,
        IK,
        UNKNOWN,
        IKChain,
        Rotated,
        IKTarget,
        Invisible,
        Twist,
        Rotated2,
    }

    public class PmdBone
    {
        public String Name { get; set; }
        public Int16? Parent { get; set; }
        public Int16? Tail { get; set; }
        public PmdBoneType BoneType { get; set; }
        public Int16? IK { get; set; }
        public Vector3 Position { get; set; }
    }

    public class PmdModel
    {
        public PmdHeader Header { get; set; }
        public PmdVertex[] Vertices { get; set; }
        public UInt16[] Indices { get; set; }
        public PmdMaterial[] Materials { get; set; }
        public PmdBone[] Bones { get; set; }
    }
}
