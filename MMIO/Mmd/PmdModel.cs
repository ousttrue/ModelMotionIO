using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
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

    public class PmdModel
    {
        public String Name { get; set; }
        public String Comment { get; set; }
        public PmdVertex[] Vertices { get; set; }
    }
}
