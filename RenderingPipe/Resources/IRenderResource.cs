using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Resources
{
    public interface IRenderResource
    {
        RenderResourceType RenderResourceType { get; }
    }
}
