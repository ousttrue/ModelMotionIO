using System;
using System.Text;

namespace MMIO.Mmd
{
    static class Int16Extensions
    {
        static public Int16? ToPmdBoneIndex(this Int16 x, bool allowZero=true)
        {
            if (allowZero)
            {
                return x != -1 ? (Int16?)x : null;
            }
            else
            {
                return x != -1 && x != 0 ? (Int16?)x : null;
            }
        }

    }

    static public class PmdParse
    {
        public static Encoding PmdEncoding = Encoding.GetEncoding(932);

        public static BParser<String> PmdString(int byteLength)
        {
            return BParse.String(byteLength, PmdEncoding);
        }

        public static BParser<PmdHeader> Header =
            from pmd in BParse.StringOf("Pmd", PmdEncoding)
            from version in BParse.SingleOf(1.0f)
            from name in PmdString(20)
            from comment in PmdString(256)
            select new PmdHeader
            {
                Version=version,
                Name=name,
                Comment=comment,
            }
            ;

        public static BParser<PmdVertex> Vertex =
            from position in BParse.Vector3
            from normal in BParse.Vector3
            from uv in BParse.Vector2
            from boneIndex0 in BParse.Int16.Select(x => x.ToPmdBoneIndex())
            from boneIndex1 in BParse.Int16.Select(x => x.ToPmdBoneIndex())
            from boneWeight0 in BParse.Byte
            from flag in BParse.Byte
            select new PmdVertex
            {
                Position = position,
                Normal = normal,
                UV = uv,
                Bone0 = boneIndex0,
                Bone1 = boneIndex1,
                BoneWeight0 = boneWeight0,
                Flag = flag,
            };

        public static BParser<PmdMaterial> Material =
            from diffuseRGBA in BParse.Vector4
            from specularity in BParse.Single
            from specularRGB in BParse.Vector3
            from ambientRGB in BParse.Vector3
            from toonIndex in BParse.Byte
            from flag in BParse.Byte
            from faceIndexCount in BParse.Int32
            from textureFile in PmdString(20)
            select new PmdMaterial
            {
                DiffuseRGBA=diffuseRGBA,
                Specularity=specularity,
                SpecularRGB=specularRGB,
                AmbientRGB=ambientRGB,
                ToonIndex=toonIndex,
                Flag=flag,
                FaceIndexCount=faceIndexCount,
                TextureFile=textureFile,
            };

        public static BParser<PmdBone> Bone =
            from name in PmdString(20)
            from parentBoneIndex in BParse.Int16.Select(x => x.ToPmdBoneIndex())
            from tailBoneIndex in BParse.Int16.Select(x => x.ToPmdBoneIndex(false))
            from boneType in BParse.Byte.Select(x => (PmdBoneType)x)
            from ikBoneIndex in BParse.Int16.Select(x => x.ToPmdBoneIndex(false))
            from position in BParse.Vector3
            select new PmdBone
            {
                Name=name,
                Parent=parentBoneIndex,
                Tail=tailBoneIndex,
                BoneType=boneType,
                IK=ikBoneIndex,
                Position=position,
            };

        public static BParser<PmdIK> IK =
            from targetBoneIndex in BParse.Int16.Select(x => x.ToPmdBoneIndex())
            from chainBoneIndex in BParse.Int16.Select(x => x.ToPmdBoneIndex())
            from chainLength in BParse.Byte
            from iterations in BParse.Int16
            from limit in BParse.Single
            from chain in BParse.Int16.Select(x => x.ToPmdBoneIndex()).Times(chainLength)
            select new PmdIK
            {
                Effector=targetBoneIndex,
                Target=chainBoneIndex,
                Iterations=iterations,
                Limit=limit,
                Chain=chain,
            };

        public static BParser<PmdVertexMorphOffset> MorphOffset =
            from index in BParse.Int32
            from offset in BParse.Vector3
            select new PmdVertexMorphOffset
            {
                VertexIndex=index,
                Offset=offset,
            };

        public static BParser<PmdMorph> Morph =
            from name in PmdString(20)
            from offsetCount in BParse.Int32
            from morphType in BParse.Byte.Select(x => (PmdMorphType)x)
            from offsets in MorphOffset.Times(offsetCount)
            select new PmdMorph
            {
                Name=name,               
                MorphType=morphType,
                Offsets=offsets,
            };

        public static BParser<PmdBoneGroup> BoneGroup =
            from boneIndex in BParse.Int16
            from boneGroupIndex in BParse.Byte
            select new PmdBoneGroup
            {
                BoneIndex=boneIndex,
                BoneGroupNameIndex=boneGroupIndex,
            };

        public static BParser<Bullet.Rigidbody> Rigidbody =
            from name in PmdString(20)
            from boneIndex in BParse.Int16
            from collisionGroup in BParse.Byte
            from ignoreGroup in BParse.UInt16
            from shapeType in BParse.Byte.Select(x => (Bullet.RigidbodyShapeType)x)
            from shapeSize in BParse.Vector3
            from position in BParse.Vector3
            from rotation in BParse.Vector3
            from mass in BParse.Single
            from linearDamping in BParse.Single
            from angularDamping in BParse.Single
            from restitution in BParse.Single
            from friction in BParse.Single
            from operationType in BParse.Byte.Select(x => (Bullet.RigidbodyOperationType)x)
            select new Bullet.Rigidbody
            {
                Name=name,
                BoneIndex=boneIndex,
                CollisionGroup=collisionGroup,
                CollisionIgnoreGroup=ignoreGroup,
                ShapeType=shapeType,
                ShapeSize=shapeSize,
                Position=position,
                EulerAngleRadians=rotation,
                Mass=mass,
                LinearDamping=linearDamping,
                AngularDamping=angularDamping,
                Restitution=restitution,
                Friction=friction,
                OperationType=operationType,
            };

        public static BParser<Bullet.Joint> Joint =
            from name in PmdString(20)
            from indexA in BParse.Int32
            from indexB in BParse.Int32
            from position in BParse.Vector3
            from rotation in BParse.Vector3
            from minTranslation in BParse.Vector3
            from maxTranslation in BParse.Vector3
            from minRotation in BParse.Vector3
            from maxRotation in BParse.Vector3
            from linearStiffness in BParse.Vector3
            from angularStiffness in BParse.Vector3
            select new Bullet.Joint
            {
                Name=name,
                RigidBodyIndexA=indexA,
                RigidBodyIndexB=indexB,
                Position=position,
                EulerAngleRadians=rotation,
                LinearLowerLimit=minTranslation,
                LinearUpperLimit=maxTranslation,
                AngularLowerLimit=minRotation,
                AngularUpperLimit=maxRotation,
                LinearStiffness=linearStiffness,
                AngularStiffness=angularStiffness,
            };

        public static BParser<PmdModel> Model = 
            from header in Header
            // vertices
            from vertexCount in BParse.Int32
            from vertices in Vertex.Times(vertexCount)
            // indices
            from indexCount in BParse.Int32
            from indices in BParse.UInt16.Times(indexCount)
            // materials
            from materialCount in BParse.Int32
            from materials in Material.Times(materialCount)
            // bones
            from boneCount in BParse.Int16
            from bones in Bone.Times(boneCount)
            // ik
            from ikCount in BParse.Int16
            from ikList in IK.Times(ikCount)
            // morph
            from morphCount in BParse.Int16
            from morphs in Morph.Times(morphCount)
            // morphGroups
            from morphGroupCount in BParse.Byte
            from morphGroups in BParse.Int16.Times(morphGroupCount)
            // boneGroupNames
            from boneGroupNameCount in BParse.Byte
            from boneGroupNames in PmdString(50).Times(boneGroupNameCount)
            // boneGroups
            from boneGroupCount in BParse.Int32
            from boneGroups in BoneGroup.Times(boneGroupCount)
            // english
            from english in (
                from englishFlag in BParse.ByteOf(1)
                from englishName in PmdString(20)
                from englishComment in PmdString(256)
                from englishBoneNames in PmdString(20).Times(boneCount)
                from englishMorphNames in PmdString(20).Times(morphCount-1)
                from englishBoneGroupNames in PmdString(50).Times(boneGroupNameCount)
                select new {
                    englishName
                    , englishComment
                    , englishBoneNames
                    , englishMorphNames
                    , englishBoneGroupNames
                }
            )
            // toon textures
            from toonTextures in PmdString(100).Times(10)
            // physics
            from rigidbodyCount in BParse.Int32
            from rigidBodies in Rigidbody.Times(rigidbodyCount)
            from jointCount in BParse.Int32
            from joints in Joint.Times(jointCount)
            select new PmdModel
            {
                Header=header,
                Vertices = vertices,
                Indices = indices,
                Materials = materials,
                Bones = bones,
                IKList=ikList,
                Morphs=morphs,
                MorphGroups=morphGroups,
                BoneGroupNames=boneGroupNames,
                BoneGroups=boneGroups,
                ToonTextures=toonTextures,
                Rigidbodies=rigidBodies,
                Joints=joints,
            };

        public static readonly BParser<PmdModel> Parser = Model;
    }
}
