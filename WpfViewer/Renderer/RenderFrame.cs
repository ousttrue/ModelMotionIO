using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer
{
    public class RenderFrame
    {
        public Resources.RenderResourceBase[] Resources;
        public Commands.IRenderCommand[] Commands;
    }
}
