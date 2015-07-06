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

    public class PmdIK
    {
        public Int16? Effector { get; set; }
        public Int16? Target { get; set; }
        public Int16 Iterations { get; set; }
        public Single Limit { get; set; }
        public Int16?[] Chain { get; set; }
    }

    public enum PmdMorphType
    {
        Base,
        Eyebrow,
        Eyes,
        Lip,
        Other,
    }

    public struct PmdVertexMorphOffset
    {
        public Int32 VertexIndex;
        public Vector3 Offset;
    }

    public class PmdMorph
    {
        public String Name { get; set; }
        public PmdMorphType MorphType { get; set; }
        public PmdVertexMorphOffset[] Offsets { get; set; }
    }

    public class PmdBoneGroup
    {
        public Int16? BoneIndex { get; set; }
        public Byte BoneGroupNameIndex { get; set; }
    }

    public class PmdModel
    {
        public PmdHeader Header { get; set; }
        public PmdVertex[] Vertices { get; set; }
        public UInt16[] Indices { get; set; }
        public PmdMaterial[] Materials { get; set; }
        public PmdBone[] Bones { get; set; }
        public PmdIK[] IKList { get; set; }
        public PmdMorph[] Morphs { get; set; }
        public Int16[] MorphGroups { get; set; }
        public String[] BoneGroupNames { get; set; }
        public PmdBoneGroup[] BoneGroups { get; set; }
        public String[] ToonTextures { get; set; }
        public Bullet.Rigidbody[] Rigidbodies { get; set; }
        public Bullet.Joint[] Joints { get; set; }
    }
}
