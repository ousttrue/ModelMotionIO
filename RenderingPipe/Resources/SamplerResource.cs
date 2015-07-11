using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources
{
    public class SamplerResource : RenderResourceBase
    {
        public override RenderResourceType RenderResourceType
        {
            get { return RenderResourceType.Sampler; }
        }

        public static SamplerResource Create()
        {
            throw new NotImplementedException();
        }
    }
}
