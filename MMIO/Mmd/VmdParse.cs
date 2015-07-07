using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    public static class VmdParse
    {
        static BParser<String> VmdString(int byteCount)
        {
            return
                from text in BParse.String(byteCount, Encoding.GetEncoding(932))
                select text;
        }

        static BParser<VmdBoneFrame> BoneFrame =
            from boneName in VmdString(15)
            from frame in BParse.Int32
            from position in BParse.Vector3
            from rotation in BParse.Quaternion
            from interpolationParams in BParse.Bytes(64).Select(x => x.ToArray())
            select new VmdBoneFrame
            {
                BoneName=boneName,
                Frame=frame,
                Position=position,
                Rotation=rotation,
                InterpolationParams=interpolationParams,
            };

        static BParser<VmdMorphFrame> MorphFrame =
            from morphName in VmdString(15)
            from frame in BParse.Int32
            from value in BParse.Single
            select new VmdMorphFrame
            {
                MorphName=morphName,
                Frame=frame,
                Value=value,
            };

        static BParser<VmdMotion> Motion =
            from signature in BParse.StringOf("Vocaloid Motion Data 0002", Encoding.ASCII, 30)
            from targetModelName in VmdString(20)
            // bone
            from boneFrameCount in BParse.Int32
            from boneFrames in BoneFrame.Times(boneFrameCount)
            // morph
            from morphFrameCount in BParse.Int32
            from morphFrames in MorphFrame.Times(morphFrameCount)
            select new VmdMotion
            {
                TargetModelName=targetModelName,
                BoneFrames=boneFrames,
                MorphFrames=morphFrames,
            };

        public static VmdMotion Execute(Byte[] bytes)
        {
            var result = Motion(new ArraySegment<byte>(bytes));

            return result.Value;
        }
    }
}
