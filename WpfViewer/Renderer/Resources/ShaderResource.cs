using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Resources
{
    public class ShaderResource : RenderResourceBase
    {
        public override RenderResourceType RenderResourceType
        {
            get { return RenderResourceType.Shader; }
        }

        public ShaderStage ShaderStage { get; set; }
        public Byte[] ByteCode { get; set; }

        public static ShaderResource Create(ShaderStage stage, Byte[] byteCode)
        {
            return new ShaderResource
            {
                ShaderStage = stage,
                ByteCode = byteCode,
            };
        }
    }
}
