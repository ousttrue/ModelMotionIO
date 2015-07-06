using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    static public class PmdParse
    {
        public static BParser<PmdHeader> Header =
            from pmd in BParse.StringOf("Pmd", Encoding.ASCII)
            from version in BParse.SingleOf(1.0f)
            from name in BParse.String(20, Encoding.GetEncoding(932))
            from comment in BParse.String(256, Encoding.GetEncoding(932))
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
            from boneIndex0 in BParse.Int16
            from boneIndex1 in BParse.Int16
            from boneWeight0 in BParse.Byte
            from flag in BParse.Byte
            select new PmdVertex
            {
                Position = position,
                Normal = normal,
                UV = uv,
                Bone0 = (boneIndex0!=-1 ? (Int16?)boneIndex0 : null),
                Bone1 = (boneIndex1!=-1 ? (Int16?)boneIndex1 : null),
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
            from textureFile in BParse.String(20, Encoding.GetEncoding(932))
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
            from name in BParse.String(20, Encoding.GetEncoding(932))
            from parentBoneIndex in BParse.Int16
            from tailBoneIndex in BParse.Int16
            from boneType in BParse.Byte
            from ikBoneIndex in BParse.Int16
            from position in BParse.Vector3
            select new PmdBone
            {
                Name=name,
                Parent=parentBoneIndex!=-1 ? (Int16?)parentBoneIndex : null,
                Tail=tailBoneIndex!=-1 && tailBoneIndex!=0 ? (Int16?)tailBoneIndex:null,
                BoneType=(PmdBoneType)boneType,
                IK=ikBoneIndex!=-1 && ikBoneIndex!=0 ? (Int16?)ikBoneIndex:null,
                Position=position,
            };

        public static BParser<PmdIK> IK =
            from targetBoneIndex in BParse.Int16
            from chainBoneIndex in BParse.Int16
            from chainLength in BParse.Byte
            from iterations in BParse.Int16
            from limit in BParse.Single
            from chain in BParse.Int16.Times(chainLength)
            select new PmdIK
            {
                Effector=targetBoneIndex!=-1 ? (Int16?)targetBoneIndex:null,
                Target=chainBoneIndex!=-1 ?(Int16?)chainBoneIndex:null,
                Iterations=iterations,
                Limit=limit,
                Chain=chain.Select(x => x!=-1 ? (Int16?)x:null).ToArray(),
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
            select new PmdModel
            {
                Header=header,
                Vertices = vertices,
                Indices = indices,
                Materials = materials,
                Bones = bones,
                IKList=ikList,
            };

        public static PmdModel Parse(Byte[] bytes)
        {
            return Model(new ArraySegment<byte>(bytes)).Value;
        }
    }
}
