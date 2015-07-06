using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMIO.Mmd
{
    static public class PmdParse
    {
        public static BParser<PmdVertex> Vertex = from position in BParse.Vector3
                                                  from normal in BParse.Vector3
                                                  from uv in BParse.Vector2
                                                  from boneIndex0 in BParse.Int16
                                                  from boneIndex1 in BParse.Int16
                                                  from boneWeight0 in BParse.Byte
                                                  from flag in BParse.Byte
                                                  select new PmdVertex
                                                  {
                                                      Position=position,
                                                      Normal=normal,
                                                      UV=uv,
                                                      BoneIndex0=boneIndex0,
                                                      BoneIndex1=boneIndex1,
                                                      BoneWeight0=boneWeight0,
                                                      Flag=flag,
                                                  };

        public static BParser<PmdModel> Model = from pmd in BParse.StringOf("Pmd", Encoding.ASCII)
                                                 from version in BParse.SingleOf(1.0f)
                                                 from name in BParse.String(20, Encoding.GetEncoding(932))
                                                 from comment in BParse.String(256, Encoding.GetEncoding(932))
                                                 from vertexCount in BParse.Int32
                                                 from vertices in Vertex.Times(vertexCount)
                                                 select new PmdModel {
                                                     Name=name,
                                                     Comment=comment,
                                                     Vertices=vertices,
                                                 };
    }
}
