using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Commands
{
    public interface IRenderCommand
    {
        RenderCommandType RenderCommandType
        {
            get;
        }
    }
}
