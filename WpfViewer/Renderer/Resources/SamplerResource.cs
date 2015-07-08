using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Resources
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
