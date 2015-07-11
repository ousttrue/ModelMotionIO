using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderingPipe.Resources;

namespace RenderingPipe.Commands
{
    public class ShaderSamplerSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.ShaderSampler_Set; }
        }

        public ShaderStage Stage { get; set; }
        public UInt32[] Samplers
        {
            get;
            private set;
        }

        public static ShaderSamplerSetCommand Create(ShaderStage stage, IEnumerable<SamplerResource> resources = null)
        {
            if (resources == null)
            {
                resources = new SamplerResource[] { };
            }
            return new ShaderSamplerSetCommand
            {
                Stage = stage,
                Samplers = resources.Select(r => r.ID).ToArray(),
            };
        }
    }
}
