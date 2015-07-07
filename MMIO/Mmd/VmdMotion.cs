using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    public struct VmdBoneFrame
    {
        public String BoneName;
        public Int32 Frame;
        public Vector3 Position;
        public Quaternion Rotation;
        public Byte[] InterpolationParams;
    }

    public struct VmdMorphFrame
    {
        public String MorphName;
        public Int32 Frame;
        public Single Value;
    }

    public struct VmdCameraFrame
    {

    }

    public struct VmdLightFrame
    {

    }

    public struct VmdSelfShadowFrame
    {

    }

    public class VmdMotion
    {
        public String TargetModelName { get; set; }
        public VmdBoneFrame[] BoneFrames { get; set; }
        public VmdMorphFrame[] MorphFrames { get; set; }
        public VmdCameraFrame[] CameraFrames { get; set; }
        public VmdLightFrame[] LightFrames { get; set; }
        public VmdSelfShadowFrame[] SelfShadowFrames { get; set; }
    }
}
