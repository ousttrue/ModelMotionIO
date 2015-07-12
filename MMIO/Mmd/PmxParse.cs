using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    public class PmxParse
    {
        #region static
        public readonly static BParser<String> PmxStringUtf16 =
            from byteLengh in BParse.Int32
            from bytes in BParse.Bytes(byteLengh).Select(x => x.ToArray())
            select Encoding.Unicode.GetString(bytes);

        public readonly static BParser<String> PmxStringUtf8 =
            from byteLengh in BParse.Int32
            from bytes in BParse.Bytes(byteLengh).Select(x => x.ToArray())
            select Encoding.UTF8.GetString(bytes);

        static readonly BParser<Tuple<PmxHeader, BParser<String>>> _Header =
            from pmx in BParse.StringOf("PMX ", Encoding.ASCII)
            from version in BParse.Single
            from flagCount in BParse.ByteOf(8)
            from flags in BParse.Byte.Times(flagCount)
            select Tuple.Create(
            new PmxHeader
            {
                Version = version,
                Flags = flags,
            }
            , flags[0] == 0 ? PmxStringUtf16 : PmxStringUtf8);

        public static readonly BParser<PmxHeader> Header =
            from headerWithPmxString in _Header
            from name in headerWithPmxString.Item2
            from englishName in headerWithPmxString.Item2
            from comment in headerWithPmxString.Item2
            from englishComment in headerWithPmxString.Item2
            select headerWithPmxString.Item1.SetNames(name, englishName, comment, englishComment);

        public static readonly BParser<PmxModel> Parser =
            from parser in Header.Select(x => new PmxParse(x))
            from model in parser.Model
            select model;

        public static PmxModel Execute(Byte[] bytes) { return Parser.Parse(bytes); }
        #endregion

        /// <summary>
        /// Headerのフラグに依存するパーサ
        /// </summary>
        #region Instance
        PmxHeader Context
        {
            get;
            set;
        }

        public PmxParse(PmxHeader context)
        {
            Context = context;
            PmxString = Context.Flags[0] == 0 ? PmxStringUtf16 : PmxStringUtf8;
            VertexIndex = Index(Context.Flags[2]);
            TextureIndex = Index(Context.Flags[3]);
            MaterialIndex = NullableIndex(Context.Flags[4]);
            BoneIndex = NullableIndex(Context.Flags[5]);
            MorphIndex = NullableIndex(Context.Flags[6]);
            RigidbodyIndex = NullableIndex(Context.Flags[7]);
        }

        BParser<String> PmxString;

        #region Index
        BParser<Int32> Index(int byteSize)
        {
            switch (byteSize)
            {
                case 1: return BParse.Byte.Select(x => (Int32)x);
                case 2: return BParse.UInt16.Select(x => (Int32)x);
                case 4: return BParse.Int32.Select(x => x);
            }
            throw new ArgumentException();
        }
        BParser<Int32?> NullableIndex(int byteSize)
        {
            switch (byteSize)
            {
                case 1: return BParse.Byte.Select(x => x != 0xFF ? (Int32?)x : null);
                case 2: return BParse.UInt16.Select(x => x != 0xFFFF ? (Int32?)x : null);
                case 4: return BParse.Int32.Select(x => x).Select(x => x != -1 ? (Int32?)x : null);
            }
            throw new ArgumentException();
        }
        BParser<Int32> VertexIndex;
        BParser<Int32> TextureIndex;
        BParser<Int32?> MaterialIndex;
        BParser<Int32?> BoneIndex;
        BParser<Int32?> MorphIndex;
        BParser<Int32?> RigidbodyIndex;
        #endregion

        #region Vertex
        BParser<PmxDeform> Deform(PmxDeformType deformType)
        {
            switch (deformType)
            {
                case PmxDeformType.BDEF1:
                    return
                    from boneIndex in BoneIndex
                    select new PmxDeform
                    {
                        DeformType = PmxDeformType.BDEF1,
                        BoneIndex0 = boneIndex,
                    };

                case PmxDeformType.BDEF2:
                    return
                    from boneIndex0 in BoneIndex
                    from boneIndex1 in BoneIndex
                    from boneWeight0 in BParse.Single
                    select new PmxDeform
                    {
                        DeformType = PmxDeformType.BDEF2,
                        BoneIndex0 = boneIndex0,
                        BoneIndex1 = boneIndex1,
                        BoneWeight0 = boneWeight0,
                        BoneWeight1 = 1.0f - boneWeight0,
                    };

                case PmxDeformType.BDEF4:
                    return
                        from boneIndex0 in BoneIndex
                        from boneIndex1 in BoneIndex
                        from boneIndex2 in BoneIndex
                        from boneIndex3 in BoneIndex
                        from boneWeight0 in BParse.Single
                        from boneWeight1 in BParse.Single
                        from boneWeight2 in BParse.Single
                        from boneWeight3 in BParse.Single
                        select new PmxDeform
                        {
                            DeformType = PmxDeformType.BDEF4,
                            BoneIndex0 = boneIndex0,
                            BoneIndex1 = boneIndex1,
                            BoneIndex2 = boneIndex2,
                            BoneIndex3 = boneIndex3,
                            BoneWeight0 = boneWeight0,
                            BoneWeight1 = boneWeight1,
                            BoneWeight2 = boneWeight2,
                            BoneWeight3 = boneWeight3,
                        };

                case PmxDeformType.SDEF:
                    return
                    from boneIndex0 in BoneIndex
                    from boneIndex1 in BoneIndex
                    from boneWeight0 in BParse.Single
                    from a in BParse.Vector3
                    from b in BParse.Vector3
                    from c in BParse.Vector3
                    // fallback to BDEF2
                    select new PmxDeform
                    {
                        DeformType = PmxDeformType.BDEF2,
                        BoneIndex0 = boneIndex0,
                        BoneIndex1 = boneIndex1,
                        BoneWeight0 = boneWeight0,
                        BoneWeight1 = 1.0f - boneWeight0,
                    };
            }
            throw new ArgumentException();
        }

        BParser<PmxVertex> Vertex
        {
            get
            {
                return
                from position in BParse.Vector3
                from normal in BParse.Vector3
                from uv in BParse.Vector2
                from extendedUV in BParse.Vector4.Times(Context.Flags[1])
                from deformType in BParse.Byte.Select(x => (PmxDeformType)x)
                from deform in Deform(deformType)
                from edgeFactor in BParse.Single
                select new PmxVertex
                {
                    Position = position,
                    Normal = normal,
                    UV = uv,
                    ExtendedUV1 = Context.Flags[1] > 0 ? extendedUV[0] : default(Vector4),
                    ExtendedUV2 = Context.Flags[1] > 1 ? extendedUV[1] : default(Vector4),
                    ExtendedUV3 = Context.Flags[1] > 2 ? extendedUV[2] : default(Vector4),
                    ExtendedUV4 = Context.Flags[1] > 3 ? extendedUV[3] : default(Vector4),
                    Deform = deform,
                    EdgeFactor = edgeFactor,
                };
            }
        }
        #endregion

        BParser<PmxMaterial> Material
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from diffuseRGBA in BParse.Vector4
                    from specularRGBA in BParse.Vector4
                    from ambientRGB in BParse.Vector3
                    from flag in BParse.Byte.Select(x => (PmxMaterialFlags)x)
                    from edgeRGBA in BParse.Vector4
                    from edgeSize in BParse.Single
                    from mainTextureIndex in TextureIndex
                    from secondaryTextureIndex in TextureIndex
                    from secondaryTextureType in BParse.Byte.Select(x => (PmxMaterialSecondaryTextureType)x)
                    from useSystemToonTexure in BParse.Byte.Select(x => x != 0)
                    from toonTextureIndex in (useSystemToonTexure ? BParse.Byte.Select(x => (Int32)x) : TextureIndex)
                    from comment in PmxString
                    from faceVertexCount in BParse.Int32
                    select new PmxMaterial
                    {
                        Name = name,
                        EnglishName = englishName,
                        DiffuseRGBA = diffuseRGBA,
                        SpecularRGBA = specularRGBA,
                        AmbientRGB = ambientRGB,
                        MaterialFlag = flag,
                        EdgeRGBA = edgeRGBA,
                        EdgeSize = edgeSize,
                        MainTextureIndex = mainTextureIndex,
                        SecondaryTextureIndex = secondaryTextureIndex,
                        SecondaryTextureType = secondaryTextureType,
                        UseSystemToonTexture = useSystemToonTexure,
                        ToonTextureIndex = toonTextureIndex,
                        Comment = comment,
                        FaceVertexCount = faceVertexCount,
                    };
            }
        }

        BParser<PmxIKLink> IKLink
        {
            get
            {
                return
                    from boneIndex in BoneIndex
                    from limitAngle in BParse.Byte.Select(x => x != 0)
                    from min in (limitAngle ? BParse.Vector3 : BParse.Return(default(Vector3)))
                    from max in (limitAngle ? BParse.Vector3 : BParse.Return(default(Vector3)))
                    select new PmxIKLink
                    {
                        BoneIndex = boneIndex,
                        LimitAngle = limitAngle,
                        LimitMinRadians = min,
                        LimitMaxRadians = max,
                    };
            }
        }

        BParser<PmxIK> IK
        {
            get
            {
                return
                    from targetIndex in BoneIndex
                    from loop in BParse.Int32
                    from limitRadians in BParse.Single
                    from linkCount in BParse.Int32
                    from links in IKLink.Times(linkCount)
                    select new PmxIK
                    {
                        TargetIndex = targetIndex,
                        Loop = loop,
                        LimitAngleCycle = limitRadians,
                        Links = links,
                    };
            }
        }

        BParser<PmxBone> Bone
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from position in BParse.Vector3
                    from parentIndex in BoneIndex
                    from layer in BParse.Int32
                    from flag in BParse.UInt16.Select(x => (PmxBoneFlags)x)
                        // tail
                    from tailOffset in flag.Has(PmxBoneFlags.HasTailBone) ? BParse.Return(default(Vector3)) : BParse.Vector3
                    from tailBoneIndex in flag.Has(PmxBoneFlags.HasTailBone) ? BoneIndex : BParse.Return<Int32?>(null)
                        // add
                    from addParentIndex in (flag.Has(PmxBoneFlags.Rotated) || flag.Has(PmxBoneFlags.Translated))
                    ? BoneIndex : BParse.Return<Int32?>(null)
                    from addRatio in (flag.Has(PmxBoneFlags.Rotated) || flag.Has(PmxBoneFlags.Translated))
                    ? BParse.Single : BParse.Return(0.0f)
                        // fixed axis
                    from fixedaxis in flag.Has(PmxBoneFlags.FixedAxis) ? BParse.Vector3 : BParse.Return(default(Vector3))
                        // local axis
                    from localX in flag.Has(PmxBoneFlags.LocalAxis) ? BParse.Vector3 : BParse.Return(default(Vector3))
                    from localZ in flag.Has(PmxBoneFlags.LocalAxis) ? BParse.Vector3 : BParse.Return(default(Vector3))
                        // external axis
                    from externalKey in flag.Has(PmxBoneFlags.ExternalParent) ? BParse.Int32 : BParse.Return(0)
                        // ik
                    from ik in flag.Has(PmxBoneFlags.IKEffector) ? IK : BParse.Return(default(PmxIK))
                    select new PmxBone
                    {
                        Name = name,
                        EnglishName = englishName,
                        Position = position,
                        ParentIndex = parentIndex,
                        Layer = layer,
                        BoneFlag = flag,
                        TailOffset = tailOffset,
                        TailIndex = tailBoneIndex,
                        AddIndex = addParentIndex,
                        AddRatio = addRatio,
                        FixedAxis = fixedaxis,
                        LocalAxisX = localX,
                        LocalAXisZ = localZ,
                        ExternalKey = externalKey,
                        IK = ik,
                    };
            }
        }

        BParser<PmxGroupMorphOffset> GroupMorphOffset
        {
            get
            {
                return
                    from morphIndex in MorphIndex
                    from value in BParse.Single
                    select new PmxGroupMorphOffset
                    {
                        MorphIndex = morphIndex,
                        Value = value,
                    };
            }
        }

        BParser<PmxVertexMorphOffset> VertexMorphOffset
        {
            get
            {
                return
                    from vertexIndex in VertexIndex
                    from position in BParse.Vector3
                    select new PmxVertexMorphOffset
                    {
                        VertexIndex = vertexIndex,
                        PositionOffset = position,
                    };
            }
        }

        BParser<PmxBoneMorphOffset> BoneMorphOffset
        {
            get
            {
                return
                    from boneIndex in BoneIndex
                    from position in BParse.Vector3
                    from rotation in BParse.Quaternion
                    select new PmxBoneMorphOffset
                    {
                        BoneIndex = boneIndex,
                        Position = position,
                        Rotation = rotation,
                    };
            }
        }

        BParser<PmxUVMorphOffset> UVMorphOffset
        {
            get
            {
                return
                    from vertexIndex in VertexIndex
                    from uv in BParse.Vector4
                    select new PmxUVMorphOffset
                    {
                        VertexIndex = vertexIndex,
                        UVOffset = uv,
                    };
            }
        }

        BParser<PmxMaterialMorphOffset> MaterialMorphOffset
        {
            get
            {
                return
                    from materialIndex in MaterialIndex
                    from operation in BParse.Byte.Select(x => (PmxMaterialMorphOperation)x)
                    from diffuseRGBA in BParse.Vector4
                    from specularRGBA in BParse.Vector4
                    from ambientRGB in BParse.Vector3
                    from edgeRGBA in BParse.Vector4
                    from edgeSize in BParse.Single
                    from mainTextureFactor in BParse.Vector4
                    from secondaryTextureFactor in BParse.Vector4
                    from toonTextureFactor in BParse.Vector4
                    select new PmxMaterialMorphOffset
                    {
                        MaterialIndex = materialIndex,
                        Operation = operation,
                        DiffuseRGBA = diffuseRGBA,
                        SpecularRGBA = specularRGBA,
                        AmbientRGB = ambientRGB,
                        EdgeRGBA = edgeRGBA,
                        EdgeSize = edgeSize,
                        MainTextureFactor = mainTextureFactor,
                        SecondaryTextureFactor = secondaryTextureFactor,
                        ToonTextureFactor = toonTextureFactor,
                    };
            }
        }

        BParser<PmxMorph> Morph
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from panel in BParse.Byte.Select(x => (PmxMorphPanel)x)
                    from morphType in BParse.Byte.Select(x => (PmxMorphType)x)
                    from offsetCount in BParse.Int32
                    from groupMorphOffsets in (morphType == PmxMorphType.Group ? GroupMorphOffset.Times(offsetCount) : BParse.Return<PmxGroupMorphOffset[]>(null))
                    from vertexMorphOffsets in (morphType == PmxMorphType.Vertex ? VertexMorphOffset.Times(offsetCount) : BParse.Return<PmxVertexMorphOffset[]>(null))
                    from boneMorphOffsets in (morphType == PmxMorphType.Bone ? BoneMorphOffset.Times(offsetCount) : BParse.Return<PmxBoneMorphOffset[]>(null))
                    from uvMorphOffsets in (morphType == PmxMorphType.UV ? UVMorphOffset.Times(offsetCount) : BParse.Return<PmxUVMorphOffset[]>(null))
                    from uvMorphOffsets1 in (morphType == PmxMorphType.ExtendedUV1 ? UVMorphOffset.Times(offsetCount) : BParse.Return<PmxUVMorphOffset[]>(null))
                    from uvMorphOffsets2 in (morphType == PmxMorphType.ExtendedUV2 ? UVMorphOffset.Times(offsetCount) : BParse.Return<PmxUVMorphOffset[]>(null))
                    from uvMorphOffsets3 in (morphType == PmxMorphType.ExtendedUV3 ? UVMorphOffset.Times(offsetCount) : BParse.Return<PmxUVMorphOffset[]>(null))
                    from uvMorphOffsets4 in (morphType == PmxMorphType.ExtendedUV4 ? UVMorphOffset.Times(offsetCount) : BParse.Return<PmxUVMorphOffset[]>(null))
                    from materialMorphOffsets in (morphType == PmxMorphType.Material ? MaterialMorphOffset.Times(offsetCount) : BParse.Return<PmxMaterialMorphOffset[]>(null))
                    select new PmxMorph
                    {
                        Name = name,
                        EnglishName = englishName,
                        MorphPanel = panel,
                        MorphType = morphType,
                        GroupMorphOffsets = groupMorphOffsets,
                        VertexMorphOffsets = vertexMorphOffsets,
                        BoneMorphOffsets = boneMorphOffsets,
                        UVMorphOffsets = uvMorphOffsets,
                        ExtendedUV1MorphOffsets = uvMorphOffsets1,
                        ExtendedUV2MorphOffsets = uvMorphOffsets2,
                        ExtendedUV3MorphOffsets = uvMorphOffsets3,
                        ExtendedUV4MorphOffsets = uvMorphOffsets4,
                        MaterialMorphOffsets = materialMorphOffsets,
                    };
            }
        }

        BParser<PmxDisplayItem> DisplayItem
        {
            get
            {
                return
                    from displayType in BParse.Byte.Select(x => (PmxDisplayType)x)
                    from index in (displayType == PmxDisplayType.Bone ? BoneIndex : MorphIndex)
                    select new PmxDisplayItem
                    {
                        DisplayType = displayType,
                        Index = index,
                    };
            }
        }

        BParser<PmxDisplayPanel> DisplayPanel
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from isSpecial in BParse.Byte.Select(x => x != 0)
                    from itemCount in BParse.Int32
                    from items in DisplayItem.Times(itemCount)
                    select new PmxDisplayPanel
                    {
                        Name = name,
                        EnglishName = englishName,
                        IsSpecial = isSpecial,
                        Items = items,
                    };
            }
        }

        BParser<Bullet.Rigidbody> Rigidbody
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from boneIndex in BoneIndex
                    from collisionGroup in BParse.Byte
                    from collisionIgnoreCollisionGroup in BParse.UInt16
                    from shapeType in BParse.Byte.Select(x => (Bullet.RigidbodyShapeType)x)
                    from shapeSize in BParse.Vector3
                    from position in BParse.Vector3
                    from rotation in BParse.Vector3
                    from mass in BParse.Single
                    from linearDampler in BParse.Single
                    from angularDampler in BParse.Single
                    from restitusion in BParse.Single
                    from friction in BParse.Single
                    from operatoinType in BParse.Byte.Select(x => (Bullet.RigidbodyOperationType)x)
                    select new Bullet.Rigidbody
                    {
                        Name = name,
                        EnglishName = englishName,
                        BoneIndex = boneIndex,
                        CollisionGroup = collisionGroup,
                        CollisionIgnoreGroup = collisionIgnoreCollisionGroup,
                        ShapeType = shapeType,
                        ShapeSize = shapeSize,
                        Position = position,
                        EulerAngleRadians = rotation,
                        Mass = mass,
                        LinearDamping = linearDampler,
                        AngularDamping = angularDampler,
                        Restitution = restitusion,
                        Friction = friction,
                        OperationType = operatoinType
                    };
            }
        }

        BParser<Bullet.Joint> Joint
        {
            get
            {
                return
                    from name in PmxString
                    from englishName in PmxString
                    from jointType in BParse.Byte.Select(x => (Bullet.JointType)x)
                    from indexA in RigidbodyIndex
                    from indexB in RigidbodyIndex
                    from position in BParse.Vector3
                    from rotation in BParse.Vector3
                    from minTranslation in BParse.Vector3
                    from maxTranslation in BParse.Vector3
                    from minRotation in BParse.Vector3
                    from maxRotation in BParse.Vector3
                    from translationStiffness in BParse.Vector3
                    from rotationStiffness in BParse.Vector3
                    select new Bullet.Joint
                    {
                        Name = name,
                        EnglishName = englishName,
                        JointType = jointType,
                        RigidBodyIndexA = indexA,
                        RigidBodyIndexB = indexB,
                        Position = position,
                        EulerAngleRadians = rotation,
                        LinearLowerLimit = minTranslation,
                        LinearUpperLimit = maxTranslation,
                        AngularLowerLimit = minRotation,
                        AngularUpperLimit = maxRotation,
                        LinearStiffness = translationStiffness,
                        AngularStiffness = rotationStiffness,
                    };
            }
        }

        /// <summary>
        /// 途中から
        /// </summary>
        BParser<PmxModel> Model
        {
            get
            {
                return
                    // vertex
                    from vertexCount in BParse.Int32
                    from vertices in Vertex.Times(vertexCount)
                        // index
                    from indexCount in BParse.Int32
                    from indices in VertexIndex.Times(indexCount)
                        // textures
                    from textureCount in BParse.Int32
                    from textures in PmxString.Times(textureCount)
                        // materials
                    from materialCount in BParse.Int32
                    from materials in Material.Times(materialCount)
                        // bones
                    from boneCount in BParse.Int32
                    from bones in Bone.Times(boneCount)
                        // morphs
                    from morphCount in BParse.Int32
                    from morphs in Morph.Times(morphCount)
                        // display
                    from displayCount in BParse.Int32
                    from displays in DisplayPanel.Times(displayCount)
                        // physics
                    from rigidbodyCount in BParse.Int32
                    from rigidbodies in Rigidbody.Times(rigidbodyCount)
                    from jointCount in BParse.Int32
                    from joints in Joint.Times(jointCount)
                        //
                    select new PmxModel
                    {
                        Header = Context,
                        Vertices = vertices,
                        Indices = indices,
                        Textures = textures,
                        Materials = materials,
                        Bones = bones,
                        Morphs = morphs,
                        Dispplays = displays,
                        Rigidbodies = rigidbodies,
                        Joints = joints,
                    };
            }
        }

        #endregion
    }
}
