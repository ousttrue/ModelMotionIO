using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    public class PmxHeader
    {
        public Single Version { get; set; }
        Byte[] m_flags;
        public Byte[] Flags {
            get { return m_flags; }
            set
            {
                if (m_flags == value) return;
                m_flags = value.ToArray();

                // encoding
                switch (m_flags[0])
                {
                    case 0:
                        Encoding = Encoding.Unicode;
                        break;

                    case 1:
                        Encoding = Encoding.UTF8;
                        break;

                    default:
                        throw new ArgumentException();
                }
            }
        }

        public String Name { get; set; }
        public String EnglishName { get; set; }
        public String Comment { get; set; }
        public String EnglishComment { get; set; }
        public PmxHeader SetNames(String name, String englishName, String comment, String englishComment)
        {
            Name = name;
            EnglishName = englishName;
            Comment = comment;
            EnglishComment = englishComment;
            return this;
        }

        public Encoding Encoding { get; private set; }

        public String DecodeString(Byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }
    }

    public enum PmxDeformType
    {
        BDEF1,
        BDEF2,
        BDEF4,
        SDEF,
        QDEF,
    }

    public struct PmxDeform
    {
        public PmxDeformType DeformType;
        public Int32? BoneIndex0;
        public Int32? BoneIndex1;
        public Int32? BoneIndex2;
        public Int32? BoneIndex3;
        public Single BoneWeight0;
        public Single BoneWeight1;
        public Single BoneWeight2;
        public Single BoneWeight3;
    }

    public struct PmxVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Vector4 ExtendedUV1;
        public Vector4 ExtendedUV2;
        public Vector4 ExtendedUV3;
        public Vector4 ExtendedUV4;
        public PmxDeform Deform;
        public Single EdgeFactor;
    }

    [Flags]
    public enum PmxMaterialFlags
    {
        None = 0x00,
        BothFace = 0x01,
        GroundShadow = 0x02,
        CastShadow = 0x04,
        SelfShadow = 0x08,
        DrawEdge = 0x10,
    }

    public enum PmxMaterialSecondaryTextureType
    {
        None,
        SphereMult,
        SphereAdd,
        ExtendedUV1,
    }

    public class PmxMaterial
    {
        public String Name { get; set; }
        public String EnglishName { get; set; }
        public Vector4 DiffuseRGBA { get; set; }
        public Vector4 SpecularRGBA { get; set; }
        public Vector3 AmbientRGB { get; set; }
        public PmxMaterialFlags MaterialFlag { get; set; }
        public Vector4 EdgeRGBA { get; set; }
        public Single EdgeSize { get; set; }
        public Int32? MainTextureIndex;
        public Int32? SecondaryTextureIndex;
        public PmxMaterialSecondaryTextureType SecondaryTextureType;
        public bool UseSystemToonTexture { get; set; }
        public Int32? ToonTextureIndex;
        public String Comment;
        public Int32 FaceVertexCount;
    }

    public class PmxIKLink
    {
        public Int32? BoneIndex { get; set; }
        public bool LimitAngle { get; set; }
        public Vector3 LimitMinRadians { get; set; }
        public Vector3 LimitMaxRadians { get; set; }
    }

    public class PmxIK
    {
        public Int32? TargetIndex { get; set; }
        public Int32 Loop { get; set; }
        public Single LimitAngleCycle { get; set; }
        public PmxIKLink[] Links { get; set; }
    }

    [Flags]
    public enum PmxBoneFlags : UInt16
    {
        None = 0x0000,
        HasTailBone = 0x0001,
        CanRotate = 0x0002,
        CanTranslate = 0x0004,
        Visible = 0x0008,
        CanManipulate = 0x0010,
        IKEffector = 0x0020,
        LocalAdd = 0x0080,
        Rotated = 0x0100,
        Translated = 0x0200,
        FixedAxis = 0x0400,
        LocalAxis = 0x0800,
        AfterPhysics = 0x1000,
        ExternalParent = 0x2000,
    }

    public static class PmxBoneFlagsExtensions
    {
        public static bool Has(this PmxBoneFlags flag, PmxBoneFlags target)
        {
            return (flag & target) == target;
        }
    }

    public class PmxBone
    {
        public String Name { get; set; }
        public String EnglishName { get; set; }
        public Vector3 Position { get; set; }
        public Int32? ParentIndex { get; set; }
        public Int32 Layer { get; set; }
        public PmxBoneFlags BoneFlag { get; set; }
    }

    public enum PmxMorphPanel
    {
        System,
        Eyebrow,
        Eyes,
        Lip,
        Other,
    }

    public enum PmxMorphType
    {
        Group,
        Vertex,
        Bone,
        UV,
        ExtendedUV1,
        ExtendedUV2,
        ExtendedUV3,
        ExtendedUV4,
        Material,
    }

    public struct PmxGroupMorphOffset
    {
        public Int32? MorphIndex;
        public Single Value;
    }

    public struct PmxVertexMorphOffset
    {
        public Int32 VertexIndex;
        public Vector3 PositionOffset;
    }

    public struct PmxBoneMorphOffset
    {
        public Int32? BoneIndex;
        public Vector3 Position;
        public Vector4 Rotation;
    }

    public struct PmxUVMorphOffset
    {
        public Int32 VertexIndex;
        public Vector4 UVOffset;
    }

    public enum PmxMaterialMorphOperation
    {
        Mul,
        Add,
    }

    public struct PmxMaterialMorphOffset
    {
        public Int32? MaterialIndex;
        public PmxMaterialMorphOperation Operation;
        public Vector4 DiffuseRGBA;
        public Vector4 SpecularRGBA;
        public Vector3 AmbientRGB;
        public Vector4 EdgeRGBA;
        public Single EdgeSize;
        public Vector4 MainTextureFactor;
        public Vector4 SecondaryTextureFactor;
        public Vector4 ToonTextureFactor;
    }

    public class PmxMorph
    {
        public String Name { get; set; }
        public String EnglishName { get; set; }
        public PmxMorphPanel MorphPanel { get; set; }
        public PmxMorphType MorphType { get; set; }
        public PmxGroupMorphOffset[] GroupMorphOffsets { get; set; }
        public PmxVertexMorphOffset[] VertexMorphOffsets { get; set; }
        public PmxBoneMorphOffset[] BoneMorphOffsets { get; set; }
        public PmxUVMorphOffset[] UVMorphOffsets { get; set; }
        public PmxUVMorphOffset[] ExtendedUV1MorphOffsets { get; set; }
        public PmxUVMorphOffset[] ExtendedUV2MorphOffsets { get; set; }
        public PmxUVMorphOffset[] ExtendedUV3MorphOffsets { get; set; }
        public PmxUVMorphOffset[] ExtendedUV4MorphOffsets { get; set; }
        public PmxMaterialMorphOffset[] MaterialMorphOffsets { get; set; }
    }

    public enum PmxDisplayType
    {
        Bone,
        Morph,
    }

    public struct PmxDisplayItem
    {
        public PmxDisplayType DisplayType { get; set; }
        public Int32? Index;
    }

    public class PmxDisplayPanel
    {
        public String Name { get; set; }
        public String EnglishName { get; set; }
        public Boolean IsSpecial { get; set; }
        public PmxDisplayItem[] Items { get; set; }
    }

    public class PmxModel
    {
        public PmxHeader Header;

        public PmxVertex[] Vertices;
        public Int32[] Indices;
        public String[] Textures;
        public PmxMaterial[] Materials;
        public PmxBone[] Bones;
        public PmxMorph[] Morphs;
        public PmxDisplayPanel[] Dispplays;
        public Bullet.Rigidbody[] Rigidbodies;
        public Bullet.Joint[] Joints;
    }
}
